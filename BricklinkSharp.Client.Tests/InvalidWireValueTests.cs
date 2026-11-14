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
using System.Text.Json;
using System.Text.Json.Nodes;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;

namespace BricklinkSharp.Client.Tests;

/// <summary>
/// Unknown or malformed wire values fail instead of silently defaulting (<c>CODING.md</c>).
/// </summary>
public class InvalidWireValueTests
{
    private const string UnknownWireValue = "\"BOGUS\"";

    private static BricklinkCredentials Credentials => BricklinkTestHost.DefaultCredentials;

    [Test]
    public void Should_ThrowJsonException_When_ConditionIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new ConditionStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_CompletenessIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new CompletenessStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_ShippingAreaIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new ShippingAreaStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_CouponStatusIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new CouponStatusStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_AppearsAsIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new AppearsAsStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_DiscountTypeIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new DiscountTypeStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_RatingTargetRoleIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new RatingTargetRoleStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_EventTypeIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new EventTypeStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_ItemTypeIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new ItemTypeStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_NullableItemTypeIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new NullableItemTypeStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_NullableCouponRestrictionTypeIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new NullableCouponRestrictionTypeStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_OrderStatusIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new OrderStatusStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_PaymentStatusIsUnknown()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new PaymentStatusStringConverter(), UnknownWireValue));
    }

    [Test]
    public void Should_ThrowJsonException_When_RatingTypeIsOutOfRange()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new RatingTypeIntConverter(), "99"));
    }

    [Test]
    public void Should_ThrowJsonException_When_DecimalIsMalformed()
    {
        var converter = new DecimalStringConverter();

        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"abc\""));
        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"\""));
    }

    [Test]
    public void Should_ThrowJsonException_When_DoubleIsMalformed()
    {
        var converter = new DoubleStringConverter();

        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"abc\""));
        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"\""));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryConditionIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "new_or_used", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryCompletenessIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "completeness", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryUnitPriceIsMalformed()
    {
        var json = Fixtures.JsonWith("Inventory", "unit_price", "n/a");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryItemTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Inventory", "item.type", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_CouponStatusIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "status", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetCouponAsync(555, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_CouponDiscountTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "discount_type", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetCouponAsync(555, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_CouponRestrictionTypeIsUnknown()
    {
        var json = Fixtures.JsonWith("Coupon", "applies_to.type", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetCouponAsync(555, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_FeedbackRatingTargetRoleIsUnknown()
    {
        var json = Fixtures.JsonWith("Feedback", "rating_of_bs", "X");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetFeedbackAsync(777, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_FeedbackRatingIsOutOfRange()
    {
        var json = Fixtures.JsonWith("Feedback", "rating", JsonValue.Create(99));
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetFeedbackAsync(777, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_OrderStatusIsUnknown()
    {
        var json = Fixtures.JsonWith("Order", "status", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetOrderAsync(1234567, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_OrderPaymentStatusIsUnknown()
    {
        var json = Fixtures.JsonWith("Order", "payment.status", "BOGUS");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetOrderAsync(1234567, Credentials));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_OrderGrandTotalIsMalformed()
    {
        var json = Fixtures.JsonWith("Order", "cost.grand_total", "n/a");
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetOrderAsync(1234567, Credentials));
    }

    [Test]
    public void Should_ThrowJsonException_When_WireStringTokenIsNotAString()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new ConditionStringConverter(), "1"));
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new DecimalStringConverter(), "1.5"));
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new DoubleStringConverter(), "1.5"));
    }

    [Test]
    public void Should_ThrowJsonException_When_RatingTokenIsNotAnInteger()
    {
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new RatingTypeIntConverter(), "\"1\""));
        Assert.Throws<JsonException>(() => ConverterHarness.Read(new RatingTypeIntConverter(), "1.5"));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_InventoryUnitPriceIsANumberToken()
    {
        var json = Fixtures.JsonWith("Inventory", "unit_price", JsonValue.Create(1.5));
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(() => host.Client.GetInventoryAsync(99001, Credentials));
    }
}
