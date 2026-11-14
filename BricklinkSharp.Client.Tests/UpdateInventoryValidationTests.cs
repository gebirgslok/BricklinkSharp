#region License
// Copyright (c) 2020 Jens Eisenbach
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using NUnit.Framework;

namespace BricklinkSharp.Client.Tests;

public class UpdateInventoryValidationTests
{
    private static UpdateInventory CreateValidWithTiers() => new()
    {
        UnitPrice = 10M,
        TierQuantity1 = 10,
        TierQuantity2 = 20,
        TierQuantity3 = 30,
        TierPrice1 = 9M,
        TierPrice2 = 8M,
        TierPrice3 = 7M
    };

    private static BricklinkInvalidParameterException ValidateAndCatch(UpdateInventory inventory)
    {
        var exception = Assert.Throws<BricklinkInvalidParameterException>(() => DtoValidation.Validate(inventory));
        Assert.That(exception!.TypeOfDataObject, Is.EqualTo(typeof(UpdateInventory)));
        return exception;
    }

    private static string ExpectedTierRulesMessage() =>
        "Following rules apply for tiered pricing: " + Environment.NewLine +
        "UnitPrice > TierPrice1 > TierPrice2 > TierPrice3," + Environment.NewLine +
        "TierQuantity1 < TierQuantity2 < TierQuantity3.";

    [Test]
    public void Should_NotThrow_When_NothingIsSet()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(new UpdateInventory()));
    }

    [Test]
    public void Should_NotThrow_When_TiersAreValid()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(CreateValidWithTiers()));
    }

    [Test]
    public void Should_Throw_When_StockRoomIdIsNullAndIsStockRoom()
    {
        var inventory = new UpdateInventory { IsStockRoom = true, StockRoomId = null };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "StockRoomId" }));
        Assert.That(exception.InvalidParameters["StockRoomId"], Is.EqualTo("null"));
        Assert.That(exception.Message, Does.StartWith("StockRoomId must be either A, B or C (set to: null."));
    }

    [Test]
    public void Should_Throw_When_StockRoomIdIsNotAbcAndIsStockRoom()
    {
        var inventory = new UpdateInventory { IsStockRoom = true, StockRoomId = "D" };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "StockRoomId" }));
        Assert.That(exception.InvalidParameters["StockRoomId"], Is.EqualTo("D"));
        Assert.That(exception.Message, Does.StartWith("StockRoomId must be either A, B or C (set to: D."));
    }

    [TestCase("A")]
    [TestCase("B")]
    [TestCase("C")]
    public void Should_NotThrow_When_StockRoomIdIsValid(string stockRoomId)
    {
        var inventory = new UpdateInventory { IsStockRoom = true, StockRoomId = stockRoomId };

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [TestCase(false)]
    [TestCase(null)]
    public void Should_NotThrow_When_StockRoomIdIsInvalidAndIsStockRoomIsFalseOrNull(bool? isStockRoom)
    {
        var inventory = new UpdateInventory { IsStockRoom = isStockRoom, StockRoomId = "D" };

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [Test]
    public void Should_Throw_When_TiersAreOnlyPartiallySet()
    {
        var inventory = new UpdateInventory { TierPrice1 = 9M, TierQuantity2 = 20 };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[]
        {
            "TierQuantity1", "TierQuantity2", "TierQuantity3", "TierPrice1", "TierPrice2", "TierPrice3"
        }));
        Assert.That(exception.InvalidParameters["TierQuantity2"], Is.EqualTo(20));
        Assert.That(exception.InvalidParameters["TierPrice1"], Is.EqualTo(9M));
        Assert.That(exception.Message, Does.StartWith(
            "All properties (TierPrice1-TierPrice3, TierQuantity1-TierQuantity3) must be setwhen using tiered pricing."));
    }

    [Test]
    public void Should_Throw_When_TierPrice3IsZeroAndOtherTierPropertiesAreSet()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierPrice3 = null;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Has.Count.EqualTo(6));
        Assert.That(exception.Message, Does.StartWith("All properties"));
    }

    [Test]
    public void Should_TreatZeroAsSet_When_CountingTierProperties()
    {
        var inventory = new UpdateInventory { TierPrice1 = 0M };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.Message, Does.StartWith("All properties"));
    }

    [Test]
    public void Should_Throw_When_TierQuantity2IsNotGreaterThanTierQuantity1()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierQuantity2 = 10;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierQuantity2" }));
        Assert.That(exception.InvalidParameters["TierQuantity2"], Is.EqualTo(10));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierQuantity3IsNotGreaterThanTierQuantity2()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierQuantity3 = 20;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierQuantity3" }));
        Assert.That(exception.InvalidParameters["TierQuantity3"], Is.EqualTo(20));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierPrice1IsNotBelowUnitPrice()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierPrice1 = 10M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice1" }));
        Assert.That(exception.InvalidParameters["TierPrice1"], Is.EqualTo(10M));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierPrice2IsNotBelowTierPrice1()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierPrice2 = 9M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice2" }));
        Assert.That(exception.InvalidParameters["TierPrice2"], Is.EqualTo(9M));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierPrice3IsNotBelowTierPrice2()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierPrice3 = 8M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice3" }));
        Assert.That(exception.InvalidParameters["TierPrice3"], Is.EqualTo(8M));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_SkipTierPrice1UpperBound_When_UnitPriceIsNull()
    {
        var inventory = CreateValidWithTiers();
        inventory.UnitPrice = null;
        inventory.TierPrice1 = 1000M;

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [Test]
    public void Should_StillCheckOtherTierRules_When_UnitPriceIsNull()
    {
        var inventory = CreateValidWithTiers();
        inventory.UnitPrice = null;
        inventory.TierPrice2 = 9M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice2" }));
    }

    [Test]
    public void Should_ReportAllOffendingTierKeys_When_SeveralRulesAreBroken()
    {
        var inventory = CreateValidWithTiers();
        inventory.TierQuantity1 = 30;
        inventory.TierQuantity2 = 30;
        inventory.TierQuantity3 = 30;
        inventory.TierPrice1 = 10M;
        inventory.TierPrice2 = 10M;
        inventory.TierPrice3 = 10M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[]
        {
            "TierQuantity2", "TierQuantity3", "TierPrice1", "TierPrice2", "TierPrice3"
        }));
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void Should_Throw_When_BulkIsLessThanOne(int bulk)
    {
        var inventory = new UpdateInventory { Bulk = bulk };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "Bulk" }));
        Assert.That(exception.InvalidParameters["Bulk"], Is.EqualTo(bulk));
        Assert.That(exception.Message, Does.StartWith("Bulk must be > 0."));
    }

    [Test]
    public void Should_NotThrow_When_BulkIsOne()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(new UpdateInventory { Bulk = 1 }));
    }

    [Test]
    public void Should_ReportOnlyStockRoomViolation_When_StockRoomTiersAndBulkAllFail()
    {
        var inventory = new UpdateInventory { IsStockRoom = true, StockRoomId = "D", TierPrice1 = 9M, Bulk = 0 };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "StockRoomId" }));
    }

    [Test]
    public void Should_ReportTierViolationOnly_When_TiersAndBulkBothFail()
    {
        var inventory = new UpdateInventory { TierPrice1 = 9M, Bulk = 0 };

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Does.Contain("TierPrice1"));
        Assert.That(exception.InvalidParameters.Keys, Does.Not.Contain("Bulk"));
    }
}
