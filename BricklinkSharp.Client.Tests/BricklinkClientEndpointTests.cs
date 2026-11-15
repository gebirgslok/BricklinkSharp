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
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class BricklinkClientEndpointTests
{
    private const string BaseUrl = BricklinkTestHost.BaseUrl;

    private static readonly BricklinkCredentials Credentials = BricklinkTestHost.DefaultCredentials;

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

        yield return new("GetCatalogItem", client => client.GetCatalogItemAsync(ItemType.Part, "3001", credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}items/{PartSegment}/3001");
        yield return new("GetSupersetsWithColor", client => client.GetSupersetsAsync(ItemType.Part, "3001", 5, credentials, cancellationToken),
            HttpMethod.Get, $"{BaseUrl}items/{PartSegment}/3001/supersets?color_id=5");
        yield return new("DeleteInventory", client => client.DeleteInventoryAsync(9, credentials, cancellationToken), HttpMethod.Delete,
            $"{BaseUrl}inventories/9");
        yield return new("GetFeedbacks", client => client.GetFeedbacksAsync(Direction.In, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}feedback?direction=in");
        yield return new("ReplyToFeedback", client => client.ReplyToFeedbackAsync(4, "thanks", credentials, cancellationToken), HttpMethod.Post,
            $"{BaseUrl}feedback/4/reply", "{\"reply\":\"thanks\"}");
        yield return new("PostFeedback", client => client.PostFeedbackAsync(11, Rating.Praise, "ok", credentials, cancellationToken),
            HttpMethod.Post, $"{BaseUrl}feedback",
            "{\"order_id\":11,\"rating\":" + System.Text.Json.JsonSerializer.Serialize(Rating.Praise) + ",\"comment\":\"ok\"}");
        yield return new("GetOrders", client => client.GetOrdersAsync(Direction.Out, null, null, true, credentials, cancellationToken),
            HttpMethod.Get, $"{BaseUrl}orders?direction={Direction.Out.ToDomainString()}&filed=true");
        yield return new("GetOrderItems", client => client.GetOrderItemsAsync(11, credentials, cancellationToken), HttpMethod.Get,
            $"{BaseUrl}orders/11/items");
        yield return new("UpdateOrderStatus", client => client.UpdateOrderStatusAsync(11, OrderStatus.Shipped, credentials, cancellationToken),
            HttpMethod.Put, $"{BaseUrl}orders/11/status",
            $"{{\"field\":\"status\",\"value\":\"{OrderStatus.Shipped.ToDomainString()}\"}}");
        yield return new("UpdatePaymentStatus", client => client.UpdatePaymentStatusAsync(11, PaymentStatus.Received, credentials, cancellationToken),
            HttpMethod.Put, $"{BaseUrl}orders/11/payment_status",
            $"{{\"field\":\"payment_status\",\"value\":\"{PaymentStatus.Received.ToDomainString()}\"}}");
        yield return new("SendDriveThru", client => client.SendDriveThruAsync(11, true, credentials, cancellationToken), HttpMethod.Post,
            $"{BaseUrl}orders/11/drive_thru?mail_me=true");
        yield return new("DeleteCoupon", client => client.DeleteCouponAsync(2, credentials, cancellationToken), HttpMethod.Delete, $"{BaseUrl}coupons/2");
    }

    [TestCaseSource(nameof(EndpointCases))]
    public void Should_SendExpectedRequest_When_EndpointIsCalled(EndpointCase endpoint)
    {
        // an error envelope ends the call after the request was captured, so no model JSON is needed
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(404));
        using var host = BricklinkTestHost.Create(handler);
        var client = host.Client;

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
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200, data)));
        var client = host.Client;

        var color = await client.GetColorAsync(5, Credentials);

        ClassicAssert.AreEqual(5, color.Id);
        ClassicAssert.AreEqual("Red", color.Name);
    }

    [Test]
    public void Should_Throw_When_ColorsAreEmpty()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200, "[]")));
        var client = host.Client;

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(() => client.GetColorsAsync(Credentials));
    }

    [Test]
    public async Task Should_ReturnEmptyArray_When_PushNotificationsAreEmpty()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200, "[]")));
        var client = host.Client;

        var notifications = await client.GetPushNotificationsAsync(Credentials);

        ClassicAssert.IsEmpty(notifications);
    }

    [Test]
    public async Task Should_ReturnBatches_When_OrderItemsAreNestedArrays()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200, "[[],[]]")));
        var client = host.Client;

        var batches = await client.GetOrderItemsAsync(11, Credentials);

        ClassicAssert.AreEqual(2, batches.Count);
    }

    [Test]
    public void Should_Throw_When_OrderItemsDataIsNotAnArray()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"a\":1}")));
        var client = host.Client;

        Assert.ThrowsAsync<BricklinkUnexpectedDataKindException>(() => client.GetOrderItemsAsync(11, Credentials));
    }

    [Test]
    public async Task Should_Complete_When_DeleteReturnsNoContentEnvelope()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(204));
        using var host = BricklinkTestHost.Create(handler);
        var client = host.Client;

        await client.DeleteCouponAsync(2, Credentials);

        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public async Task Should_Complete_When_DriveThruReturnsNoContentEnvelope()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(204)));
        var client = host.Client;

        await client.SendDriveThruAsync(11, false, Credentials);
    }

    [Test]
    public void Should_Throw_When_ReplyToFeedbackGetsUnexpectedCode()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200)));
        var client = host.Client;

        Assert.ThrowsAsync<BricklinkHttpErrorException>(() => client.ReplyToFeedbackAsync(4, "hi", Credentials));
    }

    [Test]
    public void Should_ReturnFaultedTask_When_ValidationFails()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200)));
        var client = host.Client;
        Task task = null!;

        Assert.DoesNotThrow(() => task = client.CreateInventoriesAsync(new[] { (NewInventory)null! }, Credentials));
        Assert.ThrowsAsync<NullReferenceException>(() => task);
    }

    [Test]
    public async Task Should_Complete_When_ReplyToFeedbackReturnsCreatedEnvelope()
    {
        using var host = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(201)));
        var client = host.Client;

        await client.ReplyToFeedbackAsync(4, "hi", Credentials);
    }
}
