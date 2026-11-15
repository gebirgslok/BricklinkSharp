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

    private static Endpoint<T> Get<T>(string path = "items/part/3001") =>
        Endpoint<T>.Get(ApiResourceType.Item, path);

    private static Endpoint<T> Create<T>(HttpVerb verb, string path) => verb switch
    {
        HttpVerb.Get => Endpoint<T>.Get(ApiResourceType.Item, path),
        HttpVerb.Post => Endpoint<T>.Post(ApiResourceType.Item, path),
        HttpVerb.Put => Endpoint<T>.Put(ApiResourceType.Item, path),
        _ => Endpoint<T>.Delete(ApiResourceType.Item, path)
    };

    private static BricklinkRequestExecutor CreateExecutor(StubHttpMessageHandler handler,
        IRequestHandler? requestHandler = null, BricklinkCredentials? defaultCredentials = null) =>
        BricklinkTestHost.Create(handler, requestHandler, () => defaultCredentials ?? DefaultCredentials).Executor;

    [Test]
    public async Task Should_RequestUrlWithQuery_When_RequestHasQueryParameters()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync(
            Get<Widget>().WithQuery("color_id", 5).WithQuery("region", null), null, CancellationToken.None);

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001?color_id=5",
            handler.CapturedRequests.Single().Url);
        ClassicAssert.AreEqual(HttpMethod.Get, handler.CapturedRequests.Single().Method);
    }

    [Test]
    public async Task Should_SignWithDefaultCredentials_When_NoCredentialsPassedPerCall()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var url = "https://api.bricklink.com/api/store/v1/items/part/3001";

        await CreateExecutor(handler).SendAsync(Get<Widget>(), null, CancellationToken.None);

        ClassicAssert.AreEqual(ExpectedHeader(DefaultCredentials, url, "GET"), handler.CapturedRequests.Single().Authorization);
    }

    [Test]
    public async Task Should_SignWithPerCallCredentials_When_PassedOnRequest()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var url = "https://api.bricklink.com/api/store/v1/items/part/3001";

        await CreateExecutor(handler).SendAsync(Get<Widget>(), CallCredentials, CancellationToken.None);

        ClassicAssert.AreEqual(ExpectedHeader(CallCredentials, url, "GET"), handler.CapturedRequests.Single().Authorization);
    }

    [Test]
    public async Task Should_PassResolvedDefaultCredentialsToHook_When_NoCredentialsPassedPerCall()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();

        await CreateExecutor(handler, hook).SendAsync(Get<Widget>(), null, CancellationToken.None);

        ClassicAssert.AreSame(DefaultCredentials, hook.Credentials);
        ClassicAssert.AreEqual(ApiResourceType.Item, hook.Resource);
        ClassicAssert.AreEqual(HttpVerb.Get, hook.Verb);
    }

    [Test]
    public async Task Should_PassPerCallCredentialsToHook_When_Provided()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();

        await CreateExecutor(handler, hook).SendAsync(Get<Widget>(), CallCredentials, CancellationToken.None);

        ClassicAssert.AreSame(CallCredentials, hook.Credentials);
    }

    [Test]
    public void Should_ThrowMissingCredentials_When_NoCredentialsAvailable()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var executor = BricklinkTestHost.Create(handler, credentialsProvider: () => null).Executor;

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(
            () => executor.SendAsync(Get<Widget>(), null, CancellationToken.None));
        ClassicAssert.IsEmpty(handler.CapturedRequests);
    }

    [Test]
    public void Should_ThrowMissingCredentials_When_CredentialsAreIncomplete()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(
            () => CreateExecutor(handler).SendAsync(
                Get<Widget>(), new BricklinkCredentials { ConsumerKey = "ck" }, CancellationToken.None));
    }

    [Test]
    public async Task Should_DisposeResponse_When_RequestCompletes()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync(Get<Widget>(), null, CancellationToken.None);

        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public void Should_DisposeResponse_When_ParsingFails()
    {
        var handler = new StubHttpMessageHandler("<html>boom</html>", HttpStatusCode.BadGateway);

        Assert.ThrowsAsync<BricklinkInvalidResponseException>(
            () => CreateExecutor(handler).SendAsync(Get<Widget>(), null, CancellationToken.None));
        ClassicAssert.IsTrue(handler.ResponseContent!.IsDisposed);
    }

    [Test]
    public async Task Should_OmitNullProperties_When_SerializingBody()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(201, "{\"name\":\"x\"}"));
        var request = Endpoint<Widget>.Post(ApiResourceType.Item, "items")
            .WithBody(new Widget { Name = "x", Size = null });

        await CreateExecutor(handler).SendAsync(request, null, CancellationToken.None);

        var sent = handler.CapturedRequests.Single();
        ClassicAssert.AreEqual("{\"name\":\"x\"}", sent.Body);
        ClassicAssert.AreEqual("application/json", sent.ContentType);
    }

    [Test]
    public async Task Should_SendNoContent_When_RequestHasNoBody()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(handler).SendAsync(Get<Widget>(), null, CancellationToken.None);

        ClassicAssert.IsNull(handler.CapturedRequests.Single().Body);
    }

    [Test]
    public async Task Should_IncludeQueryInSignature_When_RequestHasQuery()
    {
        var withQuery = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var withoutQuery = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));

        await CreateExecutor(withQuery).SendAsync(Get<Widget>().WithQuery("color_id", 5), null, CancellationToken.None);
        await CreateExecutor(withoutQuery).SendAsync(Get<Widget>(), null, CancellationToken.None);

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
            () => executor.SendAsync(Get<Widget>(), null, CancellationToken.None));
        ClassicAssert.IsFalse(hook.WasCalled);
    }

    [Test]
    public async Task Should_CallHookBeforeSending_When_HookIsConfigured()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();
        hook.OnCall = () => ClassicAssert.IsEmpty(handler.CapturedRequests);

        await CreateExecutor(handler, hook).SendAsync(Get<Widget>(), null, CancellationToken.None);

        ClassicAssert.IsTrue(hook.WasCalled);
    }

    [Test]
    public async Task Should_PassCancellationTokenToHook_When_Provided()
    {
        var handler = new StubHttpMessageHandler(Fixtures.Envelope(200, "{\"name\":\"x\"}"));
        var hook = new RecordingRequestHandler();
        using var source = new CancellationTokenSource();

        await CreateExecutor(handler, hook).SendAsync(Get<Widget>(), null, source.Token);

        ClassicAssert.AreEqual(source.Token, hook.Token);
    }

    private static string ExpectedHeader(BricklinkCredentials credentials, string url, string method) =>
        new OAuthRequest(credentials.ConsumerKey!, credentials.ConsumerSecret!, credentials.TokenValue!,
                credentials.TokenSecret!, url, method,
                () => DateTimeOffset.FromUnixTimeSeconds(1700000000), () => "abc123")
            .GetAuthorizationHeader();

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
