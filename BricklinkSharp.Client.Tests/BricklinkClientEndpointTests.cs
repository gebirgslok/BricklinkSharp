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
using System.Net.Http;
using System.Threading.Tasks;
using BricklinkSharp.Client.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class BricklinkClientEndpointTests
{
    private const string BaseUrl = "https://api.bricklink.com/api/store/v1/";

    private static readonly BricklinkCredentials Credentials = new()
    {
        ConsumerKey = "ck",
        ConsumerSecret = "cs",
        TokenValue = "tk",
        TokenSecret = "ts"
    };

    private static string CreateEnvelopeJson(int metaCode, string? dataJson = null) =>
        "{\"meta\":{\"description\":\"d\",\"message\":\"m\",\"code\":" + metaCode + "}" +
        (dataJson == null ? "" : ",\"data\":" + dataJson) + "}";

    private static BricklinkClient CreateClient(StubHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var options = new BricklinkRequestExecutorOptions(new Uri(BaseUrl), () => null,
            clock: () => DateTimeOffset.FromUnixTimeSeconds(1700000000), nonceGenerator: () => "abc123");
        return new BricklinkClient(new BricklinkRequestExecutor(httpClient, options), httpClient, disposeHttpClient: false);
    }

    private static string PartSegment => ItemType.Part.ToDomainString();

    public sealed record EndpointCase(string Name, Func<IBricklinkClient, Task> Call, HttpMethod Method, string Url,
        string? Body = null)
    {
        public override string ToString() => Name;
    }

    private static IEnumerable<EndpointCase> EndpointCases()
    {
        var credentials = Credentials;
        var cancellationToken = default(System.Threading.CancellationToken);

        yield return new("GetItem", client => client.GetItemAsync(ItemType.Part, "3001", credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001");
        yield return new("GetItemImage", client => client.GetItemImageAsync(ItemType.Part, "3001", 5, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001/images/5");
        yield return new("GetSupersets", client => client.GetSupersetsAsync(ItemType.Part, "3001", 0, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001/supersets");
        yield return new("GetSupersetsWithColor", client => client.GetSupersetsAsync(ItemType.Part, "3001", 5, credentials, cancellationToken),
            HttpMethod.Get, $"{BaseUrl}items/{PartSegment}/3001/supersets?color_id=5");
        yield return new("GetSubsets", client => client.GetSubsetsAsync(ItemType.Set, "10000-1", 0, true, null, false, null, credentials, cancellationToken),
            HttpMethod.Get,
            $"{BaseUrl}items/{ItemType.Set.ToDomainString()}/10000-1/subsets?color_id=0&box=true&break_minifigs=false");
        yield return new("GetPriceGuide",
            client => client.GetPriceGuideAsync(ItemType.Part, "3001", 5, PriceGuideType.Sold, Condition.Used, "DE", null, "EUR",
                null, credentials, cancellationToken),
            HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001/price?color_id=5&guide_type={PriceGuideType.Sold.ToDomainString()}" +
            $"&new_or_used={Condition.Used.ToDomainString()}&country_code=DE&currency_code=EUR");
        yield return new("GetKnownColors", client => client.GetKnownColorsAsync(ItemType.Part, "3001", credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001/colors");
        yield return new("GetColorList", client => client.GetColorListAsync(credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}colors");
        yield return new("GetColor", client => client.GetColorAsync(5, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}colors/5");
        yield return new("GetCategoryList", client => client.GetCategoryListAsync(credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}categories");
        yield return new("GetCategory", client => client.GetCategoryAsync(7, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}categories/7");
        yield return new("GetInventoryList",
            client => client.GetInventoryListAsync(new[] { ItemType.Part }, new[] { ItemType.Set }, null, null,
                new[] { 1, 2 }, new[] { 3 }, null, null, credentials, cancellationToken),
            HttpMethod.Get,
            $"{BaseUrl}inventories?item_type={PartSegment}%2C-{ItemType.Set.ToDomainString()}&category_id=1%2C2%2C-3");
        yield return new("GetInventory", client => client.GetInventoryAsync(9, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}inventories/9");
        yield return new("DeleteInventory", client => client.DeleteInventoryAsync(9, credentials, cancellationToken), HttpMethod.Delete,
            $"{BaseUrl}inventories/9");
        yield return new("GetElementId", client => client.GetElementIdAsync("3001", 5, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}item_mapping/part/3001?color_id=5");
        yield return new("GetItemNumber", client => client.GetItemNumberAsync("300101", credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}item_mapping/300101");
        yield return new("GetShippingMethodList", client => client.GetShippingMethodListAsync(credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}settings/shipping_methods");
        yield return new("GetShippingMethod", client => client.GetShippingMethodAsync(3, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}settings/shipping_methods/3");
        yield return new("GetNotifications", client => client.GetNotificationsAsync(credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}notifications");
        yield return new("GetMemberRating", client => client.GetMemberRatingAsync("bob", credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}members/bob/ratings");
        yield return new("GetFeedbackList", client => client.GetFeedbackListAsync(Direction.In, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}feedback?direction=in");
        yield return new("GetFeedback", client => client.GetFeedbackAsync(4, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}feedback/4");
        yield return new("ReplyFeedback", client => client.ReplyFeedbackAsync(4, "thanks", credentials, cancellationToken), HttpMethod.Post,
            $"{BaseUrl}feedback/4/reply", "{\"reply\":\"thanks\"}");
        yield return new("PostFeedback", client => client.PostFeedbackAsync(11, RatingType.Praise, "ok", credentials, cancellationToken),
            HttpMethod.Post, $"{BaseUrl}feedback",
            "{\"order_id\":11,\"rating\":" + System.Text.Json.JsonSerializer.Serialize(RatingType.Praise) + ",\"comment\":\"ok\"}");
        yield return new("GetOrders", client => client.GetOrdersAsync(OrderDirection.Out, null, null, true, credentials, cancellationToken),
            HttpMethod.Get, $"{BaseUrl}orders?direction={OrderDirection.Out.ToDomainString()}&filed=true");
        yield return new("GetOrder", client => client.GetOrderAsync(11, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}orders/11");
        yield return new("GetOrderItems", client => client.GetOrderItemsAsync(11, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}orders/11/items");
        yield return new("GetOrderMessages", client => client.GetOrderMessagesAsync(11, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}orders/11/messages");
        yield return new("GetOrderFeedback", client => client.GetOrderFeedbackAsync(11, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}orders/11/feedback");
        yield return new("UpdateOrderStatus", client => client.UpdateOrderStatusAsync(11, OrderStatus.Shipped, credentials, cancellationToken),
            HttpMethod.Put, $"{BaseUrl}orders/11/status",
            $"{{\"field\":\"status\",\"value\":\"{OrderStatus.Shipped.ToDomainString()}\"}}");
        yield return new("UpdatePaymentStatus", client => client.UpdatePaymentStatusAsync(11, PaymentStatus.Received, credentials, cancellationToken),
            HttpMethod.Put, $"{BaseUrl}orders/11/payment_status",
            $"{{\"field\":\"payment_status\",\"value\":\"{PaymentStatus.Received.ToDomainString()}\"}}");
        yield return new("SendDriveThru", client => client.SendDriveThruAsync(11, true, credentials, cancellationToken), HttpMethod.Post,
            $"{BaseUrl}orders/11/drive_thru?mail_me=true");
        yield return new("GetCoupons", client => client.GetCouponsAsync(Direction.Out, null, null, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}coupons?direction={Direction.Out.ToDomainString()}");
        yield return new("GetCoupon", client => client.GetCouponAsync(2, credentials, cancellationToken), HttpMethod.Get, $"{BaseUrl}coupons/2");
        yield return new("DeleteCoupon", client => client.DeleteCouponAsync(2, credentials, cancellationToken), HttpMethod.Delete, $"{BaseUrl}coupons/2");
    }

    [TestCaseSource(nameof(EndpointCases))]
    public void Should_SendExpectedRequest_When_EndpointIsCalled(EndpointCase endpoint)
    {
        // an error envelope ends the call after the request was captured, so no model JSON is needed
        var handler = new StubHttpMessageHandler(CreateEnvelopeJson(404));
        using var client = CreateClient(handler);

        Assert.ThrowsAsync<BricklinkHttpErrorException>(() => endpoint.Call(client));

        var capturedRequest = handler.CapturedRequests.Single();
        ClassicAssert.AreEqual(endpoint.Method, capturedRequest.Method);
        ClassicAssert.AreEqual(endpoint.Url, capturedRequest.Url);
        ClassicAssert.AreEqual(endpoint.Body, capturedRequest.Body);
        ClassicAssert.IsNotNull(capturedRequest.Authorization);
    }

    [Test]
    public async Task Should_MapColorFields_When_ColorIsReturned()
    {
        var data = "{\"color_id\":5,\"color_name\":\"Red\",\"color_code\":\"C91A09\",\"color_type\":\"Solid\"}";
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200, data)));

        var color = await client.GetColorAsync(5, Credentials);

        ClassicAssert.AreEqual(5, color.ColorId);
        ClassicAssert.AreEqual("Red", color.Name);
    }

    [Test]
    public void Should_Throw_When_ColorListIsEmpty()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200, "[]")));

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(() => client.GetColorListAsync(Credentials));
    }

    [Test]
    public async Task Should_ReturnEmptyArray_When_NotificationsAreEmpty()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200, "[]")));

        var notifications = await client.GetNotificationsAsync(Credentials);

        ClassicAssert.IsEmpty(notifications);
    }

    [Test]
    public async Task Should_ReturnBatches_When_OrderItemsAreNestedArrays()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200, "[[],[]]")));

        var batches = await client.GetOrderItemsAsync(11, Credentials);

        ClassicAssert.AreEqual(2, batches.Count);
    }

    [Test]
    public void Should_Throw_When_OrderItemsDataIsNotAnArray()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200, "{\"a\":1}")));

        Assert.ThrowsAsync<BricklinkUnexpectedDataKindException>(() => client.GetOrderItemsAsync(11, Credentials));
    }

    [Test]
    public async Task Should_Complete_When_DeleteReturnsNoContentCreateEnvelopeJson()
    {
        var handler = new StubHttpMessageHandler(CreateEnvelopeJson(204));
        using var client = CreateClient(handler);

        await client.DeleteCouponAsync(2, Credentials);

        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public async Task Should_Complete_When_DriveThruReturnsNoContentEnvelope()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(204)));

        await client.SendDriveThruAsync(11, false, Credentials);
    }

    [Test]
    public void Should_Throw_When_ReplyFeedbackGetsUnexpectedCode()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200)));

        Assert.ThrowsAsync<BricklinkHttpErrorException>(() => client.ReplyFeedbackAsync(4, "hi", Credentials));
    }

    [Test]
    public void Should_ReturnFaultedTask_When_ValidationFails()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(200)));
        Task task = null!;

        Assert.DoesNotThrow(() => task = client.CreateInventoriesAsync(new[] { (NewInventory)null! }, Credentials));
        Assert.ThrowsAsync<NullReferenceException>(() => task);
    }

    [Test]
    public async Task Should_Complete_When_ReplyFeedbackReturnsCreatedEnvelope()
    {
        using var client = CreateClient(new StubHttpMessageHandler(CreateEnvelopeJson(201)));

        await client.ReplyFeedbackAsync(4, "hi", Credentials);
    }
}
