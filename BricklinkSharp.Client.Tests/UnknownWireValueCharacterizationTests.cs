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
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

/// <summary>
/// Pins today's behaviour for unknown wire values, including the silent defaults that contradict
/// <c>CODING.md</c>. RFC 0003 phase 3 flips the <c>Silently*</c> tests to expect
/// <see cref="BricklinkInvalidResponseException"/>.
/// </summary>
public class UnknownWireValueCharacterizationTests
{
    private const string Unknown = "\"BOGUS\"";

    private static BricklinkCredentials Credentials => BricklinkTestHost.DefaultCredentials;

    [Test]
    public void Should_SilentlyReturnNew_When_ConditionIsUnknown()
    {
        ClassicAssert.AreEqual(Condition.New, ConverterHarness.Read(new ConditionStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnComplete_When_CompletenessIsUnknown()
    {
        ClassicAssert.AreEqual(Completeness.Complete, ConverterHarness.Read(new CompletenessStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnDomestic_When_ShippingAreaIsUnknown()
    {
        ClassicAssert.AreEqual(ShippingArea.Domestic, ConverterHarness.Read(new ShippingAreaStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnExpired_When_CouponStatusIsUnknown()
    {
        ClassicAssert.AreEqual(CouponStatus.Expired, ConverterHarness.Read(new CouponStatusStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnRegular_When_AppearsAsIsUnknown()
    {
        ClassicAssert.AreEqual(AppearsAs.Regular, ConverterHarness.Read(new AppearsAsStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnFixed_When_DiscountTypeIsUnknown()
    {
        ClassicAssert.AreEqual(DiscountType.Fixed, ConverterHarness.Read(new DiscountTypeStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnBuyer_When_RatingTargetRoleIsUnknown()
    {
        ClassicAssert.AreEqual(RatingTargetRole.Buyer, ConverterHarness.Read(new RatingTargetRoleStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnOrder_When_EventTypeIsUnknown()
    {
        ClassicAssert.AreEqual(EventType.Order, ConverterHarness.Read(new EventTypeStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnMinifig_When_NullableItemTypeIsUnknown()
    {
        ClassicAssert.AreEqual(ItemType.Minifig, ConverterHarness.Read(new NullableItemTypeStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyReturnNull_When_NullableCouponRestrictionTypeIsUnknown()
    {
        ClassicAssert.IsNull(ConverterHarness.Read(new NullableCouponRestrictionTypeStringConverter(), Unknown));
    }

    [Test]
    public void Should_SilentlyCastAnyInt_When_RatingTypeIsOutOfRange()
    {
        ClassicAssert.AreEqual((RatingType)99, ConverterHarness.Read(new RatingTypeIntConverter(), "99"));
    }

    [Test]
    public void Should_SilentlyReturnZero_When_DecimalIsMalformed()
    {
        var converter = new DecimalStringConverter();

        ClassicAssert.AreEqual(0m, ConverterHarness.Read(converter, "\"abc\""));
        ClassicAssert.AreEqual(0m, ConverterHarness.Read(converter, "\"\""));
    }

    [Test]
    public void Should_SilentlyReturnZero_When_DoubleIsMalformed()
    {
        var converter = new DoubleStringConverter();

        ClassicAssert.AreEqual(0.0, ConverterHarness.Read(converter, "\"abc\""));
        ClassicAssert.AreEqual(0.0, ConverterHarness.Read(converter, "\"\""));
    }

    [Test]
    public void Should_ThrowArgumentException_When_ItemTypeIsUnknown()
    {
        Assert.Throws<ArgumentException>(() => ConverterHarness.Read(new ItemTypeStringConverter(), Unknown));
    }

    [Test]
    public void Should_ThrowArgumentException_When_OrderStatusIsUnknown()
    {
        Assert.Throws<ArgumentException>(() => ConverterHarness.Read(new OrderStatusStringConverter(), Unknown));
    }

    [Test]
    public void Should_ThrowArgumentException_When_PaymentStatusIsUnknown()
    {
        Assert.Throws<ArgumentException>(() => ConverterHarness.Read(new PaymentStatusStringConverter(), Unknown));
    }

    [Test]
    public async Task Should_SilentlyReturnNew_When_InventoryConditionIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "new_or_used", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var inventory = await host.Client.GetInventoryAsync(99001, Credentials);

        ClassicAssert.AreEqual(Condition.New, inventory.Condition);
    }

    [Test]
    public async Task Should_SilentlyReturnComplete_When_InventoryCompletenessIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "completeness", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var inventory = await host.Client.GetInventoryAsync(99001, Credentials);

        ClassicAssert.AreEqual(Completeness.Complete, inventory.Completeness);
    }

    [Test]
    public async Task Should_SilentlyReturnZero_When_InventoryUnitPriceIsMalformed()
    {
        var json = Fixtures.JsonWith("Inventory", "unit_price", "n/a");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var inventory = await host.Client.GetInventoryAsync(99001, Credentials);

        ClassicAssert.AreEqual(0m, inventory.UnitPrice);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryItemTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "item.type", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }

    [Test]
    public async Task Should_SilentlyReturnExpired_When_CouponStatusFieldIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "status", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var coupon = await host.Client.GetCouponAsync(555, Credentials);

        ClassicAssert.AreEqual(CouponStatus.Expired, coupon.Status);
    }

    [Test]
    public async Task Should_SilentlyReturnFixed_When_CouponDiscountTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "discount_type", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var coupon = await host.Client.GetCouponAsync(555, Credentials);

        ClassicAssert.AreEqual(DiscountType.Fixed, coupon.DiscountType);
    }

    [Test]
    public async Task Should_SilentlyReturnNull_When_CouponRestrictionTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "applies_to.type", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var coupon = await host.Client.GetCouponAsync(555, Credentials);

        ClassicAssert.IsNull(coupon.AppliesTo.RestrictionType);
    }

    [Test]
    public async Task Should_SilentlyReturnBuyer_When_FeedbackRatingTargetRoleIsUnknown()
    {
        var json = Fixtures.JsonWith("Feedback", "rating_of_bs", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var feedback = await host.Client.GetFeedbackAsync(777, Credentials);

        ClassicAssert.AreEqual(RatingTargetRole.Buyer, feedback.RatingTargetRole);
    }

    [Test]
    public async Task Should_SilentlyCastAnyInt_When_FeedbackRatingIsOutOfRange()
    {
        var json = Fixtures.JsonWith("Feedback", "rating", JsonValue.Create(99));
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var feedback = await host.Client.GetFeedbackAsync(777, Credentials);

        ClassicAssert.AreEqual((RatingType)99, feedback.Rating);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_OrderResponseStatusIsUnknown()
    {
        var json = Fixtures.JsonWith("Order", "status", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetOrderAsync(1234567, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_OrderResponsePaymentStatusIsUnknown()
    {
        var json = Fixtures.JsonWith("Order", "payment.status", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetOrderAsync(1234567, Credentials));
    }

    [Test]
    public async Task Should_SilentlyReturnZero_When_OrderCostIsMalformed()
    {
        var json = Fixtures.JsonWith("Order", "cost.grand_total", "n/a");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var order = await host.Client.GetOrderAsync(1234567, Credentials);

        ClassicAssert.AreEqual(0m, order.Cost.GrandTotal);
    }
}
