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

public class NewInventoryValidationTests
{
    private static NewInventory CreateValid() => new()
    {
        Item = new ItemBase { Type = ItemType.Part },
        UnitPrice = 10M
    };

    private static NewInventory CreateValidWithTiers()
    {
        var inventory = CreateValid();
        inventory.TierQuantity1 = 10;
        inventory.TierQuantity2 = 20;
        inventory.TierQuantity3 = 30;
        inventory.TierPrice1 = 9M;
        inventory.TierPrice2 = 8M;
        inventory.TierPrice3 = 7M;
        return inventory;
    }

    private static BricklinkInvalidParameterException ValidateAndCatch(NewInventory inventory)
    {
        var exception = Assert.Throws<BricklinkInvalidParameterException>(() => DtoValidation.Validate(inventory));
        Assert.That(exception!.TypeOfDataObject, Is.EqualTo(typeof(NewInventory)));
        return exception;
    }

    private static string ExpectedTierRulesMessage() =>
        "Following rules apply for tiered pricing: " + Environment.NewLine +
        "UnitPrice > TierPrice1 > TierPrice2 > TierPrice3," + Environment.NewLine +
        "TierQuantity1 < TierQuantity2 < TierQuantity3.";

    [Test]
    public void Should_NotThrow_When_InventoryIsValid()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(CreateValid()));
    }

    [Test]
    public void Should_NotThrow_When_TiersAreValid()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(CreateValidWithTiers()));
    }

    [TestCase(ItemType.Part, Completeness.Incomplete)]
    [TestCase(ItemType.Minifig, Completeness.Sealed)]
    public void Should_Throw_When_NonSetItemIsIncompleteOrSealed(ItemType itemType, Completeness completeness)
    {
        var inventory = CreateValid();
        inventory.Item.Type = itemType;
        inventory.Completeness = completeness;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "Item.Type", "Completeness" }));
        Assert.That(exception.InvalidParameters["Item.Type"], Is.EqualTo(itemType));
        Assert.That(exception.InvalidParameters["Completeness"], Is.EqualTo(completeness));
        Assert.That(exception.Message, Does.StartWith($"{completeness} can only be applied to items of type Set."));
    }

    [TestCase(Completeness.Complete)]
    [TestCase(Completeness.Incomplete)]
    [TestCase(Completeness.Sealed)]
    public void Should_NotThrow_When_ItemIsSet(Completeness completeness)
    {
        var inventory = CreateValid();
        inventory.Item.Type = ItemType.Set;
        inventory.Completeness = completeness;

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Should_Throw_When_StockRoomIdIsMissingAndItemIsInStockRoom(string? stockRoomId)
    {
        var inventory = CreateValid();
        inventory.IsStockRoom = true;
        inventory.StockRoomId = stockRoomId;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "StockRoomId" }));
        Assert.That(exception.InvalidParameters["StockRoomId"], Is.EqualTo("Null"));
        Assert.That(exception.Message, Does.StartWith("StockRoomId cannot be Null if IsStockRoom is true."));
    }

    [Test]
    public void Should_Throw_When_StockRoomIdIsNotAbcAndIsStockRoom()
    {
        var inventory = CreateValid();
        inventory.IsStockRoom = true;
        inventory.StockRoomId = "D";

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
        var inventory = CreateValid();
        inventory.IsStockRoom = true;
        inventory.StockRoomId = stockRoomId;

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [Test]
    public void Should_NotThrow_When_StockRoomIdIsInvalidAndIsStockRoomIsFalse()
    {
        var inventory = CreateValid();
        inventory.IsStockRoom = false;
        inventory.StockRoomId = "D";

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [Test]
    public void Should_Throw_When_TiersAreOnlyPartiallySet()
    {
        var inventory = CreateValid();
        inventory.TierPrice1 = 9M;
        inventory.TierQuantity2 = 20;

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
        inventory.TierPrice3 = 0M;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Has.Count.EqualTo(6));
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
        var inventory = CreateValid();
        inventory.Bulk = bulk;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "Bulk" }));
        Assert.That(exception.InvalidParameters["Bulk"], Is.EqualTo(bulk));
        Assert.That(exception.Message, Does.StartWith("Bulk must be > 0."));
    }

    [Test]
    public void Should_NotThrow_When_BulkIsOne()
    {
        var inventory = CreateValid();
        inventory.Bulk = 1;

        Assert.DoesNotThrow(() => DtoValidation.Validate(inventory));
    }

    [Test]
    public void Should_ReportCompletenessViolation_When_AllValidationGroupsFail()
    {
        var inventory = CreateValid();
        inventory.Completeness = Completeness.Sealed;
        inventory.IsStockRoom = true;
        inventory.StockRoomId = null;
        inventory.TierPrice1 = 9M;
        inventory.Bulk = 0;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "Item.Type", "Completeness" }));
    }

    [Test]
    public void Should_ReportOnlyStockRoomViolation_When_StockRoomTiersAndBulkAllFail()
    {
        var inventory = CreateValid();
        inventory.IsStockRoom = true;
        inventory.StockRoomId = "D";
        inventory.TierPrice1 = 9M;
        inventory.Bulk = 0;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "StockRoomId" }));
    }

    [Test]
    public void Should_ReportTierViolationOnly_When_TiersAndBulkBothFail()
    {
        var inventory = CreateValid();
        inventory.TierPrice1 = 9M;
        inventory.Bulk = 0;

        var exception = ValidateAndCatch(inventory);

        Assert.That(exception.InvalidParameters!.Keys, Does.Contain("TierPrice1"));
        Assert.That(exception.InvalidParameters.Keys, Does.Not.Contain("Bulk"));
    }
}
