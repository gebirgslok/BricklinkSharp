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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointSenderTests
{
    private static readonly BricklinkCredentials CallCredentials = new()
    {
        ConsumerKey = "ck2",
        ConsumerSecret = "cs2",
        TokenValue = "tk2",
        TokenSecret = "ts2"
    };

    private static IEndpointSender CreateSender(StubHttpMessageHandler handler,
        IRequestHandler? requestHandler = null, System.Func<BricklinkCredentials?>? credentialsProvider = null) =>
        BricklinkTestHost.Create(handler, requestHandler, credentialsProvider).Executor;

    private static BricklinkInvalidParameterException CreateRejection() =>
        new(new Dictionary<string, object> { { "x", 1 } }, "bad", typeof(Widget));

    [Test]
    public async Task Should_SendUrlVerbAndBody_When_EndpointDescribesThem()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(201, "{\"name\":\"x\"}"));
        var endpoint = Endpoint<Widget>.Post(ApiResourceType.Item, "items/part/3001")
            .WithQuery("color_id", 5)
            .WithQuery("region", null)
            .WithBody(new ReplyBody { Reply = "thanks" });

        await CreateSender(handler).SendAsync(endpoint, null, CancellationToken.None);

        var sent = handler.CapturedRequests.Single();
        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001?color_id=5", sent.Url);
        ClassicAssert.AreEqual(HttpMethod.Post, sent.Method);
        ClassicAssert.AreEqual("{\"reply\":\"thanks\"}", sent.Body);
    }

    [Test]
    public async Task Should_ExpectEndpointMetaCode_When_ExplicitCodeIsGiven()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(202, "{\"name\":\"x\"}"));
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "items", expectedMetaCode: 202);

        var widget = await CreateSender(handler).SendAsync(endpoint, null, CancellationToken.None);

        ClassicAssert.AreEqual("x", widget.Name);
    }

    [Test]
    public void Should_ParseWithEndpointShape_When_ShapeIsGiven()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "[]"));
        var endpoint = Endpoint<Widget[]>.Get(ApiResourceType.Item, "items", ResponseShape.ArrayNonEmpty);

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(
            () => CreateSender(handler).SendAsync(endpoint, null, CancellationToken.None));
    }

    [Test]
    public async Task Should_SignWithPerCallCredentials_When_PassedToSender()
    {
        var withCall = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var withDefault = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "items");

        await CreateSender(withCall).SendAsync(endpoint, CallCredentials, CancellationToken.None);
        await CreateSender(withDefault).SendAsync(endpoint, null, CancellationToken.None);

        ClassicAssert.AreNotEqual(withDefault.CapturedRequests.Single().Authorization,
            withCall.CapturedRequests.Single().Authorization);
    }

    [Test]
    public async Task Should_PassCredentialsAndTokenToHook_When_Sending()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingHook();
        using var source = new CancellationTokenSource();
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "items");

        await CreateSender(handler, hook).SendAsync(endpoint, CallCredentials, source.Token);

        ClassicAssert.AreSame(CallCredentials, hook.Credentials);
        ClassicAssert.AreEqual(source.Token, hook.Token);
    }

    [Test]
    public void Should_FaultWithRejection_When_EndpointIsRejected()
    {
        var rejection = CreateRejection();

        var task = CreateSender(new StubHttpMessageHandler(Fixtures.Envelope(200)))
            .SendAsync(Endpoint<Widget>.Rejected(rejection), null, CancellationToken.None);

        ClassicAssert.IsTrue(task.IsFaulted);
        ClassicAssert.AreSame(rejection, task.Exception!.InnerException);
    }

    [Test]
    public void Should_MakeNoHttpCallAndSkipHook_When_EndpointIsRejected()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200));
        var hook = new RecordingHook();

        // no credentials available either: the rejection must win over a missing-credentials failure
        var sender = CreateSender(handler, hook, () => null);

        Assert.ThrowsAsync<BricklinkInvalidParameterException>(
            () => sender.SendAsync(Endpoint<Widget>.Rejected(CreateRejection()), null, CancellationToken.None));
        ClassicAssert.IsEmpty(handler.CapturedRequests);
        ClassicAssert.IsFalse(hook.WasCalled);
    }

    [TestCase(HttpVerb.Get, 200)]
    [TestCase(HttpVerb.Post, 201)]
    [TestCase(HttpVerb.Put, 200)]
    [TestCase(HttpVerb.Delete, 204)]
    public void Should_ResolveDefaultMetaCode_When_ResultCarriesData(HttpVerb verb, int code)
    {
        ClassicAssert.AreEqual(code, CreateEndpoint<Widget>(verb).ExpectedMetaCode);
    }

    [Test]
    public void Should_Expect204_When_PutReturnsNoData()
    {
        ClassicAssert.AreEqual(204, CreateEndpoint<NoData>(HttpVerb.Put).ExpectedMetaCode);
    }

    [Test]
    public void Should_ResolveShapeFromResultType_When_NoShapeIsGiven()
    {
        ClassicAssert.AreEqual(ResponseShape.Object, Endpoint<Widget>.Get(ApiResourceType.Item, "x").Shape);
        ClassicAssert.AreEqual(ResponseShape.ArrayOrEmpty, Endpoint<Widget[]>.Get(ApiResourceType.Item, "x").Shape);
        ClassicAssert.AreEqual(ResponseShape.NoData, Endpoint<NoData>.Get(ApiResourceType.Item, "x").Shape);
        ClassicAssert.AreEqual(ResponseShape.ArrayNonEmpty,
            Endpoint<Widget[]>.Get(ApiResourceType.Item, "x", ResponseShape.ArrayNonEmpty).Shape);
    }

    [Test]
    public void Should_DropNullQueryValues_When_BuildingEndpoint()
    {
        var endpoint = Endpoint<Widget>.Get(ApiResourceType.Item, "x")
            .WithQuery("a", 1).WithQuery("b", null).WithQuery("c", true);

        CollectionAssert.AreEqual(new[] { "a=1", "c=true" }, endpoint.Query.Pairs.Select(p => p.Key + "=" + p.Value));
    }

    [Test]
    public void Should_SerializeWireNames_When_UsingWireJson()
    {
        ClassicAssert.AreEqual("{\"field\":\"status\",\"value\":\"PAID\"}",
            WireJson.Serialize(new FieldChange { Field = "status", Value = "PAID" }));
        ClassicAssert.AreEqual("{\"reply\":\"hi\"}", WireJson.Serialize(new ReplyBody { Reply = "hi" }));
    }

    private static Endpoint<TResult> CreateEndpoint<TResult>(HttpVerb verb) => verb switch
    {
        HttpVerb.Get => Endpoint<TResult>.Get(ApiResourceType.Item, "x"),
        HttpVerb.Post => Endpoint<TResult>.Post(ApiResourceType.Item, "x"),
        HttpVerb.Put => Endpoint<TResult>.Put(ApiResourceType.Item, "x"),
        _ => Endpoint<TResult>.Delete(ApiResourceType.Item, "x")
    };

    private sealed class Widget
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class RecordingHook : IRequestHandler
    {
        public BricklinkCredentials? Credentials { get; private set; }

        public CancellationToken Token { get; private set; }

        public bool WasCalled { get; private set; }

        public Task BeforeRequestAsync(ApiResourceType resourceType, HttpVerb verb,
            BricklinkCredentials? credentials, CancellationToken cancellationToken)
        {
            Credentials = credentials;
            Token = cancellationToken;
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
