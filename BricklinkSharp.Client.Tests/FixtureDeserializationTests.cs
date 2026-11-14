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
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class FixtureDeserializationTests
{
    private static BricklinkCredentials Credentials => BricklinkTestHost.DefaultCredentials;

    private static DateTime Utc(int year, int month, int day, int hour, int minute, int second) =>
        new(year, month, day, hour, minute, second, DateTimeKind.Utc);

    [Test]
    public async Task Should_MapEveryField_When_OrderDetailsAreReturned()
    {
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(Fixtures.Json("Order")));

        var order = await host.Client.GetOrderAsync(1234567, Credentials);

        ClassicAssert.AreEqual(1234567, order.Id);
        ClassicAssert.AreEqual(Utc(2023, 5, 1, 12, 30, 45), order.OrderedOn);
        ClassicAssert.AreEqual(Utc(2023, 5, 2, 8, 0, 0), order.StatusChangedOn);
        ClassicAssert.AreEqual("seller1", order.SellerName);
        ClassicAssert.AreEqual("buyer1", order.BuyerName);
        ClassicAssert.AreEqual("buyer1@example.com", order.BuyerEmail);
        ClassicAssert.AreEqual(3, order.BuyerOrderCount);
        ClassicAssert.IsTrue(order.RequireInsurance);
        ClassicAssert.AreEqual(OrderStatus.Shipped, order.Status);
        ClassicAssert.IsTrue(order.IsInvoiced);
        ClassicAssert.IsTrue(order.IsFiled);
        ClassicAssert.IsTrue(order.WasDriveThruSent);
        ClassicAssert.IsTrue(order.WasSalesTaxCollectedByBricklink);
        ClassicAssert.AreEqual("fragile", order.Remarks);
        ClassicAssert.AreEqual(12, order.TotalCount);
        ClassicAssert.AreEqual(4, order.UniqueCount);
        ClassicAssert.AreEqual(123.45, order.TotalWeight);

        ClassicAssert.AreEqual("PayPal", order.Payment.Method);
        ClassicAssert.AreEqual("EUR", order.Payment.CurrencyCode);
        ClassicAssert.AreEqual(Utc(2023, 5, 1, 13, 0, 0), order.Payment.PaidOn);
        ClassicAssert.AreEqual(PaymentStatus.Received, order.Payment.Status);

        ClassicAssert.AreEqual("Standard", order.Shipping.Method);
        ClassicAssert.AreEqual(7, order.Shipping.ShippingMethodId);
        ClassicAssert.AreEqual("TRK123", order.Shipping.TrackingNo);
        ClassicAssert.AreEqual("https://tracking.example.com/TRK123", order.Shipping.TrackingLink);
        ClassicAssert.AreEqual(Utc(2023, 5, 2, 9, 0, 0), order.Shipping.ShippedOn);
        ClassicAssert.AreEqual("Jane Doe", order.Shipping.Address.Name.FullName);
        ClassicAssert.AreEqual("Jane", order.Shipping.Address.Name.FirstName);
        ClassicAssert.AreEqual("Doe", order.Shipping.Address.Name.LastName);
        ClassicAssert.AreEqual("Jane Doe, Main St 1, Berlin", order.Shipping.Address.FullAddress);
        ClassicAssert.AreEqual("Main St 1", order.Shipping.Address.Address1);
        ClassicAssert.AreEqual("Floor 2", order.Shipping.Address.Address2);
        ClassicAssert.AreEqual("DE", order.Shipping.Address.CountryCode);
        ClassicAssert.AreEqual("Berlin", order.Shipping.Address.City);
        ClassicAssert.AreEqual("BE", order.Shipping.Address.State);
        ClassicAssert.AreEqual("10115", order.Shipping.Address.PostalCode);
        ClassicAssert.AreEqual("123456", order.Shipping.Address.PhoneNumber);

        AssertCost(order.Cost, "EUR", 10.50m, 14.75m, 0.25m, 15.00m, 0.10m, 0.20m, 1.00m, 3.50m, 0.30m, 0.40m, 19.00m,
            2.35m);
        AssertCost(order.DisplayCost, "USD", 11.50m, 15.75m, 0.35m, 16.00m, 0.11m, 0.21m, 1.10m, 3.60m, 0.31m, 0.41m,
            19.10m, 2.45m);
    }

    private static void AssertCost(Cost cost, string currency, decimal subtotal, decimal grandTotal, decimal salesTax,
        decimal finalTotal, decimal etc1, decimal etc2, decimal insurance, decimal shipping, decimal credit,
        decimal coupon, decimal vatRate, decimal vatAmount)
    {
        ClassicAssert.AreEqual(currency, cost.CurrencyCode);
        ClassicAssert.AreEqual(subtotal, cost.Subtotal);
        ClassicAssert.AreEqual(grandTotal, cost.GrandTotal);
        ClassicAssert.AreEqual(salesTax, cost.SalesTaxCollectedByBricklink);
        ClassicAssert.AreEqual(finalTotal, cost.FinalTotal);
        ClassicAssert.AreEqual(etc1, cost.Etc1);
        ClassicAssert.AreEqual(etc2, cost.Etc2);
        ClassicAssert.AreEqual(insurance, cost.InsuranceCost);
        ClassicAssert.AreEqual(shipping, cost.ShippingCost);
        ClassicAssert.AreEqual(credit, cost.Credit);
        ClassicAssert.AreEqual(coupon, cost.CouponDiscount);
        ClassicAssert.AreEqual(vatRate, cost.VatRate);
        ClassicAssert.AreEqual(vatAmount, cost.VatAmount);
    }

    [Test]
    public async Task Should_MapKeyFields_When_OrderListIsReturned()
    {
        using var host = BricklinkTestHost.Create(Fixtures.Envelope("[" + Fixtures.Json("Order") + "]"));

        var orders = await host.Client.GetOrdersAsync(Direction.In, null, null, false, Credentials);

        var order = orders[0];
        ClassicAssert.AreEqual(1234567, order.Id);
        ClassicAssert.AreEqual(OrderStatus.Shipped, order.Status);
        ClassicAssert.AreEqual(PaymentStatus.Received, order.Payment.Status);
        ClassicAssert.AreEqual("EUR", order.Cost.CurrencyCode);
        ClassicAssert.AreEqual(10.50m, order.Cost.Subtotal);
        ClassicAssert.AreEqual(14.75m, order.Cost.GrandTotal);
        ClassicAssert.AreEqual("USD", order.DisplayCost.CurrencyCode);
        ClassicAssert.AreEqual(15.75m, order.DisplayCost.GrandTotal);
    }

    [Test]
    public async Task Should_MapEveryField_When_InventoryIsReturned()
    {
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(Fixtures.Json("Inventory")));

        var inventory = await host.Client.GetInventoryAsync(99001, Credentials);

        ClassicAssert.AreEqual(99001, inventory.Id);
        ClassicAssert.AreEqual("3001", inventory.Item.ItemNumber);
        ClassicAssert.AreEqual(ItemType.Part, inventory.Item.Type);
        ClassicAssert.AreEqual("Brick 2 x 4", inventory.Item.Name);
        ClassicAssert.AreEqual(5, inventory.Item.CategoryId);
        ClassicAssert.AreEqual(11, inventory.ColorId);
        ClassicAssert.AreEqual("Black", inventory.ColorName);
        ClassicAssert.AreEqual(42, inventory.Quantity);
        ClassicAssert.AreEqual(Condition.Used, inventory.Condition);
        ClassicAssert.AreEqual(Completeness.Sealed, inventory.Completeness);
        ClassicAssert.AreEqual(0.15m, inventory.UnitPrice);
        ClassicAssert.AreEqual(7, inventory.BindId);
        ClassicAssert.AreEqual("used, clean", inventory.Description);
        ClassicAssert.AreEqual("box 3", inventory.Remarks);
        ClassicAssert.AreEqual(10, inventory.BulkQuantity);
        ClassicAssert.IsTrue(inventory.RetainsWhenSoldOut);
        ClassicAssert.IsTrue(inventory.IsStockRoom);
        ClassicAssert.AreEqual("B", inventory.StockRoomId);
        ClassicAssert.AreEqual(Utc(2023, 4, 1, 10, 0, 0), inventory.CreatedOn);
        ClassicAssert.AreEqual(0.05m, inventory.OwnCost);
        ClassicAssert.AreEqual(20, inventory.SaleRatePercent);
        ClassicAssert.AreEqual(100, inventory.TierQuantity1);
        ClassicAssert.AreEqual(200, inventory.TierQuantity2);
        ClassicAssert.AreEqual(300, inventory.TierQuantity3);
        ClassicAssert.AreEqual(0.14m, inventory.TierPrice1);
        ClassicAssert.AreEqual(0.13m, inventory.TierPrice2);
        ClassicAssert.AreEqual(0.12m, inventory.TierPrice3);
        ClassicAssert.AreEqual(2.35, inventory.OwnWeight);
    }

    [Test]
    public async Task Should_MapEveryField_When_CouponIsReturned()
    {
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(Fixtures.Json("Coupon")));

        var coupon = await host.Client.GetCouponAsync(555, Credentials);

        ClassicAssert.AreEqual(555, coupon.Id);
        ClassicAssert.AreEqual(Utc(2023, 3, 1, 0, 0, 0), coupon.IssuedOn);
        ClassicAssert.AreEqual(Utc(2023, 6, 1, 0, 0, 0), coupon.ExpiresOn);
        ClassicAssert.AreEqual("buyer1", coupon.BuyerName);
        ClassicAssert.AreEqual("seller1", coupon.SellerName);
        ClassicAssert.AreEqual("Store One", coupon.StoreName);
        ClassicAssert.AreEqual(CouponStatus.Redeemed, coupon.Status);
        ClassicAssert.AreEqual(1234567, coupon.OrderId);
        ClassicAssert.AreEqual("spring sale", coupon.Remarks);
        ClassicAssert.AreEqual(CouponRestrictionType.ExcludeSpecifiedType, coupon.AppliesTo.RestrictionType);
        ClassicAssert.AreEqual(ItemType.Minifig, coupon.AppliesTo.ItemType);
        ClassicAssert.AreEqual(true, coupon.AppliesTo.ExceptOnSale);
        ClassicAssert.AreEqual(DiscountType.Percentage, coupon.DiscountType);
        ClassicAssert.AreEqual(5.00m, coupon.DiscountAmount);
        ClassicAssert.AreEqual(10, coupon.DiscountRate);
        ClassicAssert.AreEqual(20.00m, coupon.MaxDiscountAmount);
        ClassicAssert.AreEqual(50.00m, coupon.TierPrice1);
        ClassicAssert.AreEqual(5, coupon.TierDiscountRate1);
        ClassicAssert.AreEqual(100.00m, coupon.TierPrice2);
        ClassicAssert.AreEqual(10, coupon.TierDiscountRate2);
        ClassicAssert.AreEqual(200.00m, coupon.TierPrice3);
        ClassicAssert.AreEqual(15, coupon.TierDiscountRate3);
        ClassicAssert.AreEqual("USD", coupon.DisplayCurrencyCode);
        ClassicAssert.AreEqual(5.50m, coupon.DisplayDiscountAmount);
        ClassicAssert.AreEqual(22.00m, coupon.DisplayMaxDiscountAmount);
        ClassicAssert.AreEqual(55.00m, coupon.DisplayTierPrice1);
        ClassicAssert.AreEqual(110.00m, coupon.DisplayTierPrice2);
        ClassicAssert.AreEqual(220.00m, coupon.DisplayTierPrice3);
    }

    [Test]
    public async Task Should_LeaveAppliesToMembersNull_When_CouponAppliesToIsEmpty()
    {
        var json = Fixtures.JsonWith("Coupon", "applies_to", JsonNode.Parse("{\"type\":\"\",\"item_type\":\"\"}"));
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(json));

        var coupon = await host.Client.GetCouponAsync(555, Credentials);

        ClassicAssert.IsNull(coupon.AppliesTo.RestrictionType);
        ClassicAssert.IsNull(coupon.AppliesTo.ItemType);
        ClassicAssert.IsNull(coupon.AppliesTo.ExceptOnSale);
    }

    [Test]
    public async Task Should_MapEveryField_When_FeedbackIsReturned()
    {
        using var host = BricklinkTestHost.Create(Fixtures.Envelope(Fixtures.Json("Feedback")));

        var feedback = await host.Client.GetFeedbackAsync(777, Credentials);

        ClassicAssert.AreEqual(777, feedback.Id);
        ClassicAssert.AreEqual(1234567, feedback.OrderId);
        ClassicAssert.AreEqual("buyer1", feedback.From);
        ClassicAssert.AreEqual("seller1", feedback.To);
        ClassicAssert.AreEqual(Utc(2023, 5, 10, 11, 0, 0), feedback.CreatedOn);
        ClassicAssert.AreEqual(FeedbackTargetRole.Seller, feedback.FeedbackTargetRole);
        ClassicAssert.AreEqual(Rating.Complaint, feedback.Rating);
        ClassicAssert.AreEqual("as described", feedback.Comment);
        ClassicAssert.AreEqual("thanks", feedback.Reply);
    }
}
