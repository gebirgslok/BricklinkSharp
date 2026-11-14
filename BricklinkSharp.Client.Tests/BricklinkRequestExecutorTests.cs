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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.OAuth;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class BricklinkRequestExecutorTests
{
    private static readonly BricklinkCredentials DefaultCredentials = BricklinkTestHost.DefaultCredentials;

    private static readonly BricklinkCredentials CallCredentials = new()
    {
        ConsumerKey = "ck2",
        ConsumerSecret = "cs2",
        TokenValue = "tk2",
        TokenSecret = "ts2"
    };

    private static BricklinkRequest Get(string path = "items/part/3001") =>
        new(ApiResourceType.Item, HttpVerb.Get, path);

    private static BricklinkRequestExecutor CreateExecutor(StubHttpMessageHandler handler,
        IRequestHandler? requestHandler = null, BricklinkCredentials? defaultCredentials = null) =>
        BricklinkTestHost.Create(handler, requestHandler, () => defaultCredentials ?? DefaultCredentials).Executor;

    [Test]
    public async Task Should_RequestUrlWithQuery_When_RequestHasQueryParameters()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync<Widget>(
            Get().WithQueryParameter("color_id", 5).WithQueryParameter("region", null), CancellationToken.None);

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001?color_id=5",
            handler.CapturedRequests.Single().Url);
        ClassicAssert.AreEqual(HttpMethod.Get, handler.CapturedRequests.Single().Method);
    }

    [Test]
    public async Task Should_SignWithDefaultCredentials_When_NoCredentialsPassedPerCall()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var url = "https://api.bricklink.com/api/store/v1/items/part/3001";

        await CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.AreEqual(ExpectedHeader(DefaultCredentials, url, "GET"), handler.CapturedRequests.Single().Authorization);
    }

    [Test]
    public async Task Should_SignWithPerCallCredentials_When_PassedOnRequest()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var url = "https://api.bricklink.com/api/store/v1/items/part/3001";

        await CreateExecutor(handler).SendAsync<Widget>(Get().WithCredentials(CallCredentials), CancellationToken.None);

        ClassicAssert.AreEqual(ExpectedHeader(CallCredentials, url, "GET"), handler.CapturedRequests.Single().Authorization);
    }

    [Test]
    public async Task Should_PassResolvedDefaultCredentialsToHook_When_NoCredentialsPassedPerCall()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();

        await CreateExecutor(handler, hook).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.AreSame(DefaultCredentials, hook.Credentials);
        ClassicAssert.AreEqual(ApiResourceType.Item, hook.Resource);
        ClassicAssert.AreEqual(HttpVerb.Get, hook.Verb);
    }

    [Test]
    public async Task Should_PassPerCallCredentialsToHook_When_Provided()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();

        await CreateExecutor(handler, hook).SendAsync<Widget>(Get().WithCredentials(CallCredentials), CancellationToken.None);

        ClassicAssert.AreSame(CallCredentials, hook.Credentials);
    }

    [Test]
    public void Should_ThrowMissingCredentials_When_NoCredentialsAvailable()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var executor = BricklinkTestHost.Create(handler, credentialsProvider: () => null).Executor;

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(
            () => executor.SendAsync<Widget>(Get(), CancellationToken.None));
        ClassicAssert.IsEmpty(handler.CapturedRequests);
    }

    [Test]
    public void Should_ThrowMissingCredentials_When_CredentialsAreIncomplete()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(
            () => CreateExecutor(handler).SendAsync<Widget>(
                Get().WithCredentials(new BricklinkCredentials { ConsumerKey = "ck" }), CancellationToken.None));
    }

    [Test]
    public async Task Should_DisposeResponse_When_RequestCompletes()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public void Should_DisposeResponse_When_ParsingFails()
    {
        var handler = new StubHttpMessageHandler("<html>boom</html>", HttpStatusCode.BadGateway);

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None));
        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public void Should_ThrowBricklinkException_When_BodyIsNotJson()
    {
        var handler = new StubHttpMessageHandler("<html>Too many requests</html>", HttpStatusCode.TooManyRequests);

        var exception = Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None))!;

        ClassicAssert.IsInstanceOf<BricklinkException>(exception);
        ClassicAssert.AreEqual(429, exception.HttpStatusCode);
        StringAssert.Contains("Too many requests", exception.Message);
        ClassicAssert.IsNotNull(exception.InnerException);
    }

    [TestCase("")]
    [TestCase("[]")]
    [TestCase("{\"data\":{}}")]
    [TestCase("{\"meta\":\"x\"}")]
    public void Should_ThrowInvalidResponse_When_MetaIsMissing(string body)
    {
        var handler = new StubHttpMessageHandler(body);

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None));
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_MetaCodeIsNotANumber()
    {
        var handler = new StubHttpMessageHandler("{\"meta\":{\"code\":\"x\"}}");

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None));
    }

    [Test]
    public void Should_ThrowHttpError_When_MetaCodeDiffersFromExpected()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(404));

        var exception = Assert.ThrowsAsync<BricklinkHttpErrorException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None))!;

        ClassicAssert.AreEqual(404, exception.ReceivedStatusCode);
        ClassicAssert.AreEqual(200, exception.ExpectedStatusCode);
        ClassicAssert.AreEqual("d", exception.Description);
        ClassicAssert.AreEqual("m", exception.RawMessage);
    }

    [Test]
    public async Task Should_MapObject_When_DataIsObject()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"brick\",\"size\":3}"));

        var widget = await CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.AreEqual("brick", widget.Name);
        ClassicAssert.AreEqual(3, widget.Size);
    }

    [TestCase(null)]
    [TestCase("null")]
    [TestCase("{}")]
    public void Should_ThrowNoDataReceived_When_ObjectExpectedButDataIsEmpty(string? data)
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, data));

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None));
    }

    [TestCase("\"text\"")]
    [TestCase("42")]
    [TestCase("[1]")]
    public void Should_ThrowUnexpectedDataKind_When_ObjectExpectedButDataIsNotObject(string data)
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, data));

        var exception = Assert.ThrowsAsync<BricklinkUnexpectedDataKindException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None))!;

        ClassicAssert.AreEqual("Object", exception.ExpectedDataKind);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_DataCannotBeMappedToType()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"size\":\"not a number\"}"));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None));
    }

    [Test]
    public async Task Should_MapArray_When_DataIsArray()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "[{\"name\":\"a\"},{\"name\":\"b\"}]"));

        var widgets = await CreateExecutor(handler).SendAsync<Widget[]>(Get(), CancellationToken.None);

        CollectionAssert.AreEqual(new[] { "a", "b" }, widgets.Select(widget => widget.Name));
    }

    [Test]
    public async Task Should_ReturnEmptyArray_When_ArrayTypeAndDataIsEmptyArray()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "[]"));

        var widgets = await CreateExecutor(handler).SendAsync<Widget[]>(Get(), CancellationToken.None);

        ClassicAssert.IsEmpty(widgets);
    }

    [Test]
    public void Should_ThrowNoDataReceived_When_NonEmptyArrayRequiredButDataIsEmptyArray()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "[]"));
        var request = Get().WithShape(ResponseShape.ArrayNonEmpty);

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(
            () => CreateExecutor(handler).SendAsync<Widget[]>(request, CancellationToken.None));
    }

    [Test]
    public void Should_ThrowUnexpectedDataKind_When_ArrayExpectedButDataIsObject()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        var exception = Assert.ThrowsAsync<BricklinkUnexpectedDataKindException>(
            () => CreateExecutor(handler).SendAsync<Widget[]>(Get(), CancellationToken.None))!;

        ClassicAssert.AreEqual("Array", exception.ExpectedDataKind);
    }

    [Test]
    public void Should_ThrowNoDataReceived_When_ArrayExpectedButDataIsAbsent()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200));

        Assert.ThrowsAsync<BricklinkNoDataReceivedException>(
            () => CreateExecutor(handler).SendAsync<Widget[]>(Get(), CancellationToken.None));
    }

    [TestCase(null)]
    [TestCase("null")]
    [TestCase("{}")]
    public async Task Should_Succeed_When_NoDataExpectedAndDataIsEmpty(string? data)
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(204, data));
        var request = new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Delete, "inventories/1");

        await CreateExecutor(handler).SendAsync<NoData>(request, CancellationToken.None);

        ClassicAssert.AreEqual(HttpMethod.Delete, handler.CapturedRequests.Single().Method);
    }

    [Test]
    public void Should_ThrowEmptyDataExpected_When_NoDataExpectedButObjectHasContent()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(204, "{\"name\":\"x\"}"));
        var request = new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Delete, "inventories/1");

        Assert.ThrowsAsync<BricklinkEmptyDataExpectedException>(
            () => CreateExecutor(handler).SendAsync<NoData>(request, CancellationToken.None));
    }

    [Test]
    public void Should_ThrowUnexpectedDataKind_When_NoDataExpectedButDataIsArray()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(204, "[1]"));
        var request = new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Delete, "inventories/1");

        Assert.ThrowsAsync<BricklinkUnexpectedDataKindException>(
            () => CreateExecutor(handler).SendAsync<NoData>(request, CancellationToken.None));
    }

    [TestCase(HttpVerb.Get, 200)]
    [TestCase(HttpVerb.Put, 200)]
    [TestCase(HttpVerb.Post, 201)]
    public async Task Should_ExpectDefaultCodeForVerb_When_ReadingObject(HttpVerb verb, int code)
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(code, "{\"name\":\"x\"}"));
        var request = new BricklinkRequest(ApiResourceType.Item, verb, "items");

        var widget = await CreateExecutor(handler).SendAsync<Widget>(request, CancellationToken.None);

        ClassicAssert.AreEqual("x", widget.Name);
    }

    [TestCase(HttpVerb.Put, 204)]
    [TestCase(HttpVerb.Delete, 204)]
    [TestCase(HttpVerb.Post, 201)]
    public async Task Should_ExpectDefaultCodeForVerb_When_NoDataExpected(HttpVerb verb, int code)
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(code));
        var request = new BricklinkRequest(ApiResourceType.Item, verb, "items");

        await CreateExecutor(handler).SendAsync<NoData>(request, CancellationToken.None);
    }

    [Test]
    public async Task Should_UseExplicitCode_When_ExpectCodeIsSet()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(202, "{\"name\":\"x\"}"));

        var widget = await CreateExecutor(handler).SendAsync<Widget>(Get().WithExpectedMetaCode(202), CancellationToken.None);

        ClassicAssert.AreEqual("x", widget.Name);
    }

    [Test]
    public async Task Should_OmitNullProperties_When_SerializingBody()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(201, "{\"name\":\"x\"}"));
        var request = new BricklinkRequest(ApiResourceType.Item, HttpVerb.Post, "items")
            .WithBody(new Widget { Name = "x", Size = null });

        await CreateExecutor(handler).SendAsync<Widget>(request, CancellationToken.None);

        var sent = handler.CapturedRequests.Single();
        ClassicAssert.AreEqual("{\"name\":\"x\"}", sent.Body);
        ClassicAssert.AreEqual("application/json", sent.ContentType);
    }

    [Test]
    public async Task Should_SendNoContent_When_RequestHasNoBody()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.IsNull(handler.CapturedRequests.Single().Body);
    }

    [Test]
    public async Task Should_IncludeQueryInSignature_When_RequestHasQuery()
    {
        var withQuery = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var withoutQuery = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(withQuery).SendAsync<Widget>(Get().WithQueryParameter("color_id", 5), CancellationToken.None);
        await CreateExecutor(withoutQuery).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.AreNotEqual(withoutQuery.CapturedRequests.Single().Authorization,
            withQuery.CapturedRequests.Single().Authorization);
    }

    [Test]
    public void Should_NotCallHook_When_CredentialsAreMissing()
    {
        var hook = new RecordingRequestHandler();
        var executor = BricklinkTestHost.Create(new StubHttpMessageHandler(Fixtures.Envelope(200)), hook,
            () => null).Executor;

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(
            () => executor.SendAsync<Widget>(Get(), CancellationToken.None));
        ClassicAssert.IsFalse(hook.WasCalled);
    }

    [Test]
    public async Task Should_CallHookBeforeSending_When_HookIsConfigured()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();
        hook.OnCall = () => ClassicAssert.IsEmpty(handler.CapturedRequests);

        await CreateExecutor(handler, hook).SendAsync<Widget>(Get(), CancellationToken.None);

        ClassicAssert.IsTrue(hook.WasCalled);
    }

    [Test]
    public async Task Should_PassCancellationTokenToHook_When_Provided()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();
        using var source = new CancellationTokenSource();

        await CreateExecutor(handler, hook).SendAsync<Widget>(Get(), source.Token);

        ClassicAssert.AreEqual(source.Token, hook.Token);
    }

    [Test]
    public void Should_ThrowInvalidResponse_When_ConverterRejectsValue()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"type\":\"BOGUS\"}"));

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync<TypedWidget>(Get(), CancellationToken.None));
    }

    [Test]
    public async Task Should_MapList_When_ResultTypeIsList()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "[{\"name\":\"a\"}]"));

        var widgets = await CreateExecutor(handler).SendAsync<List<Widget>>(Get(), CancellationToken.None);

        ClassicAssert.AreEqual(1, widgets.Count);
    }

    [Test]
    public void Should_ThrowInvalidOperation_When_NoDataShapeUsedWithOtherResultType()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200));

        Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateExecutor(handler).SendAsync<Widget>(Get().WithShape(ResponseShape.NoData), CancellationToken.None));
    }

    private static string ExpectedHeader(BricklinkCredentials credentials, string url, string method) =>
        new OAuthRequest(credentials.ConsumerKey!, credentials.ConsumerSecret!, credentials.TokenValue!,
                credentials.TokenSecret!, url, method,
                () => DateTimeOffset.FromUnixTimeSeconds(1700000000), () => "abc123")
            .GetAuthorizationHeader();

    private sealed class TypedWidget
    {
        [JsonPropertyName("type"), JsonConverter(typeof(ItemTypeStringConverter))]
        public ItemType Type { get; set; }
    }

    private sealed class Widget
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("size")]
        public int? Size { get; set; }
    }

    private sealed class RecordingRequestHandler : IRequestHandler
    {
        public ApiResourceType Resource { get; private set; }

        public HttpVerb Verb { get; private set; }

        public BricklinkCredentials? Credentials { get; private set; }

        public bool WasCalled { get; private set; }

        public CancellationToken Token { get; private set; }

        public Action? OnCall { get; set; }

        public Task BeforeRequestAsync(ApiResourceType resourceType, HttpVerb verb,
            BricklinkCredentials? credentials, CancellationToken cancellationToken)
        {
            Resource = resourceType;
            Verb = verb;
            Credentials = credentials;
            WasCalled = true;
            Token = cancellationToken;
            OnCall?.Invoke();
            return Task.CompletedTask;
        }
    }
}
