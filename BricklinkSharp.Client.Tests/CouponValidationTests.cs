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

public class CouponValidationTests
{
    private static UpdateCoupon CreateValidPercentageCoupon() => new()
    {
        DiscountType = DiscountType.Percentage,
        TierPrice1 = 10M,
        TierPrice2 = 20M,
        TierPrice3 = 30M,
        TierDiscountRate1 = 10,
        TierDiscountRate2 = 20,
        TierDiscountRate3 = 30
    };

    private static BricklinkInvalidParameterException ValidateAndCatch(CouponTerms coupon)
    {
        var exception = Assert.Throws<BricklinkInvalidParameterException>(() => DtoValidation.Validate(coupon));
        Assert.That(exception!.TypeOfDataObject, Is.EqualTo(coupon.GetType()));
        return exception;
    }

    private static string ExpectedTierRulesMessage() =>
        "Following rules apply for tiered pricing: " + Environment.NewLine +
        "TierPrice1 < TierPrice2 < TierPrice3," + Environment.NewLine +
        "TierDiscountRate1 < TierDiscountRate2 < TierDiscountRate3.";

    [Test]
    public void Should_NotThrow_When_NoTiersAreSet()
    {
        var coupon = new UpdateCoupon { DiscountType = DiscountType.Percentage };

        Assert.DoesNotThrow(() => DtoValidation.Validate(coupon));
    }

    [Test]
    public void Should_NotThrow_When_TiersAreValid()
    {
        Assert.DoesNotThrow(() => DtoValidation.Validate(CreateValidPercentageCoupon()));
    }

    [Test]
    public void Should_NotThrow_When_DiscountTypeIsFixedAndTiersAreInvalid()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.DiscountType = DiscountType.Fixed;
        coupon.TierPrice2 = 5M;
        coupon.TierDiscountRate3 = 0;

        Assert.DoesNotThrow(() => DtoValidation.Validate(coupon));
    }

    [Test]
    public void Should_Throw_When_TiersAreOnlyPartiallySetOnUpdateCoupon()
    {
        var coupon = new UpdateCoupon
        {
            DiscountType = DiscountType.Percentage,
            TierPrice1 = 10M,
            TierDiscountRate2 = 20
        };

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[]
        {
            "TierDiscountRate1", "TierDiscountRate2", "TierDiscountRate3", "TierPrice1", "TierPrice2", "TierPrice3"
        }));
        Assert.That(exception.InvalidParameters["TierDiscountRate2"], Is.EqualTo(20));
        Assert.That(exception.InvalidParameters["TierPrice1"], Is.EqualTo(10M));
        Assert.That(exception.Message, Does.StartWith(
            "All properties (TierPrice1-TierPrice3, TierDiscountRate1-TierDiscountRate3) must be setwhen using tiered pricing."));
    }

    [Test]
    public void Should_Throw_When_TiersAreOnlyPartiallySetOnNewCoupon()
    {
        var coupon = new NewCoupon("buyer", "remarks")
        {
            DiscountType = DiscountType.Percentage,
            TierDiscountRate1 = 10
        };

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Has.Count.EqualTo(6));
        Assert.That(exception.Message, Does.StartWith("All properties"));
    }

    [Test]
    public void Should_Throw_When_TierPrice3IsZeroAndOtherTierPropertiesAreSet()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierPrice3 = 0M;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Has.Count.EqualTo(6));
        Assert.That(exception.Message, Does.StartWith("All properties"));
    }

    [Test]
    public void Should_Throw_When_TierDiscountRate2IsNotGreaterThanTierDiscountRate1()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierDiscountRate2 = 10;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierDiscountRate2" }));
        Assert.That(exception.InvalidParameters["TierDiscountRate2"], Is.EqualTo(10));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierDiscountRate3IsNotGreaterThanTierDiscountRate2()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierDiscountRate3 = 20;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierDiscountRate3" }));
        Assert.That(exception.InvalidParameters["TierDiscountRate3"], Is.EqualTo(20));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierPrice2IsNotGreaterThanTierPrice1()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierPrice2 = 10M;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice2" }));
        Assert.That(exception.InvalidParameters["TierPrice2"], Is.EqualTo(10M));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_Throw_When_TierPrice3IsNotGreaterThanTierPrice2()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierPrice3 = 20M;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[] { "TierPrice3" }));
        Assert.That(exception.InvalidParameters["TierPrice3"], Is.EqualTo(20M));
        Assert.That(exception.Message, Does.StartWith(ExpectedTierRulesMessage()));
    }

    [Test]
    public void Should_ReportAllOffendingTierKeys_When_SeveralRulesAreBroken()
    {
        var coupon = CreateValidPercentageCoupon();
        coupon.TierPrice1 = 30M;
        coupon.TierPrice2 = 30M;
        coupon.TierPrice3 = 30M;
        coupon.TierDiscountRate1 = 30;
        coupon.TierDiscountRate2 = 30;
        coupon.TierDiscountRate3 = 30;

        var exception = ValidateAndCatch(coupon);

        Assert.That(exception.InvalidParameters!.Keys, Is.EqualTo(new[]
        {
            "TierDiscountRate2", "TierDiscountRate3", "TierPrice2", "TierPrice3"
        }));
    }
}
