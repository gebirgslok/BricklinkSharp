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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BricklinkSharp.Client.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointsWriteTests
{
    private static readonly string[] BodyCarryingPrefixes = { "Create", "Update", "Post", "Reply", "Send" };

    // Endpoints named like a write that have nothing a caller could get wrong.
    private static readonly string[] ExemptFromValidation =
    {
        nameof(Endpoints.UpdateOrderStatus), nameof(Endpoints.UpdatePaymentStatus), nameof(Endpoints.SendDriveThru)
    };

    public sealed record InvalidCase(string Name, Func<Exception?> Rejection)
    {
        public override string ToString() => Name;
    }

    private static InvalidCase Invalid<T>(string name, Endpoint<T> endpoint) => new(name, () => endpoint.Rejection);

    private static NewInventory ValidNewInventory() => new()
    {
        Item = new Item { ItemNumber = "3001", Type = ItemType.Part },
        UnitPrice = 1.5M,
        Quantity = 3
    };

    private static NewInventory StockRoomWithoutId() =>
        new() { Item = new Item { ItemNumber = "3001", Type = ItemType.Part }, IsStockRoom = true };

    private static IEnumerable<InvalidCase> InvalidCases()
    {
        yield return Invalid(nameof(Endpoints.CreateInventory), Endpoints.CreateInventory(StockRoomWithoutId()));
        yield return Invalid(nameof(Endpoints.CreateInventories),
            Endpoints.CreateInventories(new[] { ValidNewInventory(), StockRoomWithoutId() }));
        yield return Invalid(nameof(Endpoints.UpdateInventory),
            Endpoints.UpdateInventory(1, new UpdateInventory { IsStockRoom = true }));
        yield return Invalid(nameof(Endpoints.CreateCoupon),
            Endpoints.CreateCoupon(new NewCoupon("buyer", "remarks")
            { DiscountType = DiscountType.Percentage, TierDiscountRate1 = 10 }));
        yield return Invalid(nameof(Endpoints.UpdateCoupon),
            Endpoints.UpdateCoupon(1, new UpdateCoupon { DiscountType = DiscountType.Percentage, TierPrice1 = 10M }));
        yield return Invalid(nameof(Endpoints.UpdateOrder), Endpoints.UpdateOrder(1, null!));
        yield return Invalid(nameof(Endpoints.PostFeedback), Endpoints.PostFeedback(1, Rating.Praise, " "));
        yield return Invalid(nameof(Endpoints.PostFeedback) + "UndefinedRating", Endpoints.PostFeedback(1, (Rating)999, "x"));
        yield return Invalid(nameof(Endpoints.ReplyToFeedback), Endpoints.ReplyToFeedback(1, ""));
    }

    [TestCaseSource(nameof(InvalidCases))]
    public void Should_RejectEndpoint_When_BodyIsInvalid(InvalidCase invalidCase)
    {
        ClassicAssert.IsInstanceOf<BricklinkInvalidParameterException>(invalidCase.Rejection());
    }

    [Test]
    public void Should_RejectWithNullReference_When_BatchContainsNull()
    {
        var endpoint = Endpoints.CreateInventories(new[] { (NewInventory)null! });

        ClassicAssert.IsInstanceOf<NullReferenceException>(endpoint.Rejection);
    }

    [Test]
    public void Should_ValidateOrBeExempt_When_EndpointCarriesBody()
    {
        var bodyCarrying = typeof(Endpoints)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(method => method.Name)
            .Where(name => BodyCarryingPrefixes.Any(name.StartsWith))
            .Distinct();

        var covered = InvalidCases().Select(invalidCase => invalidCase.Name).Concat(ExemptFromValidation).ToList();

        CollectionAssert.IsSubsetOf(bodyCarrying, covered);
    }

    private static string BodyJson<T>(Endpoint<T> endpoint)
    {
        ClassicAssert.IsNull(endpoint.Rejection);
        return WireJson.Serialize(endpoint.Body!);
    }

    [Test]
    public void Should_UseWireNames_When_OrderStatusIsUpdated()
    {
        var endpoint = Endpoints.UpdateOrderStatus(11, OrderStatus.Shipped);

        ClassicAssert.AreEqual(HttpVerb.Put, endpoint.Verb);
        ClassicAssert.AreEqual("orders/11/status", endpoint.Path);
        ClassicAssert.AreEqual($"{{\"field\":\"status\",\"value\":\"{OrderStatus.Shipped.ToDomainString()}\"}}",
            BodyJson(endpoint));
    }

    [Test]
    public void Should_UseWireNames_When_PaymentStatusIsUpdated()
    {
        var endpoint = Endpoints.UpdatePaymentStatus(11, PaymentStatus.Received);

        ClassicAssert.AreEqual("orders/11/payment_status", endpoint.Path);
        ClassicAssert.AreEqual(
            $"{{\"field\":\"payment_status\",\"value\":\"{PaymentStatus.Received.ToDomainString()}\"}}",
            BodyJson(endpoint));
    }

    [Test]
    public void Should_UseWireNames_When_FeedbackIsPosted()
    {
        var endpoint = Endpoints.PostFeedback(11, Rating.Praise, "ok");

        ClassicAssert.AreEqual(HttpVerb.Post, endpoint.Verb);
        ClassicAssert.AreEqual("feedback", endpoint.Path);
        ClassicAssert.AreEqual(
            "{\"order_id\":11,\"rating\":" + JsonSerializer.Serialize(Rating.Praise) + ",\"comment\":\"ok\"}",
            BodyJson(endpoint));
    }

    [Test]
    public void Should_UseWireNames_When_FeedbackIsReplied()
    {
        var endpoint = Endpoints.ReplyToFeedback(4, "thanks");

        ClassicAssert.AreEqual("feedback/4/reply", endpoint.Path);
        ClassicAssert.AreEqual("{\"reply\":\"thanks\"}", BodyJson(endpoint));
    }

    [Test]
    public void Should_ExpectNoContentAndSendQuery_When_DriveThruIsSent()
    {
        var endpoint = Endpoints.SendDriveThru(11, true);

        ClassicAssert.AreEqual(HttpVerb.Post, endpoint.Verb);
        ClassicAssert.AreEqual("orders/11/drive_thru", endpoint.Path);
        ClassicAssert.AreEqual(204, endpoint.ExpectedMetaCode);
        ClassicAssert.IsNull(endpoint.Body);
        CollectionAssert.AreEqual(new[] { new KeyValuePair<string, string>("mail_me", "true") }, endpoint.Query.Pairs);
    }

    [Test]
    public void Should_SerializeItemAndPrice_When_InventoryIsCreated()
    {
        var endpoint = Endpoints.CreateInventory(ValidNewInventory());
        using var json = JsonDocument.Parse(BodyJson(endpoint));

        ClassicAssert.AreEqual(HttpVerb.Post, endpoint.Verb);
        ClassicAssert.AreEqual("inventories", endpoint.Path);
        ClassicAssert.AreEqual(201, endpoint.ExpectedMetaCode);
        ClassicAssert.AreEqual("3001", json.RootElement.GetProperty("item").GetProperty("no").GetString());
        ClassicAssert.AreEqual(1.5M, decimal.Parse(json.RootElement.GetProperty("unit_price").GetString()!, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Test]
    public void Should_SerializeArrayWithoutResponseData_When_InventoriesAreCreated()
    {
        var endpoint = Endpoints.CreateInventories(new[] { ValidNewInventory(), ValidNewInventory() });
        using var json = JsonDocument.Parse(BodyJson(endpoint));

        ClassicAssert.AreEqual(ResponseShape.NoData, endpoint.Shape);
        ClassicAssert.AreEqual(2, json.RootElement.GetArrayLength());
    }

    [Test]
    public void Should_PutPartialBody_When_InventoryIsUpdated()
    {
        var endpoint = Endpoints.UpdateInventory(9, new UpdateInventory { UnitPrice = 2M, Remarks = "r" });
        using var json = JsonDocument.Parse(BodyJson(endpoint));

        ClassicAssert.AreEqual(HttpVerb.Put, endpoint.Verb);
        ClassicAssert.AreEqual("inventories/9", endpoint.Path);
        ClassicAssert.AreEqual(200, endpoint.ExpectedMetaCode);
        ClassicAssert.AreEqual(2M, json.RootElement.GetProperty("unit_price").GetDecimal());
        ClassicAssert.AreEqual("r", json.RootElement.GetProperty("remarks").GetString());
        ClassicAssert.IsFalse(json.RootElement.TryGetProperty("description", out _));
    }

    [Test]
    public void Should_DeleteWithoutBody_When_InventoryIsDeleted()
    {
        var endpoint = Endpoints.DeleteInventory(9);

        ClassicAssert.AreEqual(HttpVerb.Delete, endpoint.Verb);
        ClassicAssert.AreEqual("inventories/9", endpoint.Path);
        ClassicAssert.AreEqual(204, endpoint.ExpectedMetaCode);
        ClassicAssert.IsNull(endpoint.Body);
    }

    [Test]
    public void Should_SerializeBuyerName_When_CouponIsCreated()
    {
        var endpoint = Endpoints.CreateCoupon(new NewCoupon("buyer", "remarks"));
        using var json = JsonDocument.Parse(BodyJson(endpoint));

        ClassicAssert.AreEqual("coupons", endpoint.Path);
        ClassicAssert.AreEqual("buyer", json.RootElement.GetProperty("buyer_name").GetString());
    }

    [Test]
    public void Should_PutToCouponPath_When_CouponIsUpdated()
    {
        var endpoint = Endpoints.UpdateCoupon(2, new UpdateCoupon());

        ClassicAssert.AreEqual(HttpVerb.Put, endpoint.Verb);
        ClassicAssert.AreEqual("coupons/2", endpoint.Path);
        ClassicAssert.IsNull(endpoint.Rejection);
        ClassicAssert.IsNotNull(endpoint.Body);
    }

    [Test]
    public void Should_DeleteWithoutBody_When_CouponIsDeleted()
    {
        var endpoint = Endpoints.DeleteCoupon(2);

        ClassicAssert.AreEqual("coupons/2", endpoint.Path);
        ClassicAssert.AreEqual(HttpVerb.Delete, endpoint.Verb);
        ClassicAssert.IsNull(endpoint.Body);
    }

    [Test]
    public void Should_SerializeChangedFields_When_OrderIsUpdated()
    {
        var endpoint = Endpoints.UpdateOrder(11, new UpdateOrder { Remarks = "note", IsFiled = true });
        using var json = JsonDocument.Parse(BodyJson(endpoint));

        ClassicAssert.AreEqual(HttpVerb.Put, endpoint.Verb);
        ClassicAssert.AreEqual("orders/11", endpoint.Path);
        ClassicAssert.AreEqual("note", json.RootElement.GetProperty("remarks").GetString());
        ClassicAssert.IsTrue(json.RootElement.GetProperty("is_filed").GetBoolean());
    }
}
