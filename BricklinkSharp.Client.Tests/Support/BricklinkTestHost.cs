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
using System.Linq;
using System.Net.Http;

namespace BricklinkSharp.Client.Tests.Support;

/// <summary>
/// Builds the stub handler, executor and client with OAuth set up once, so tests only supply the response body.
/// </summary>
internal sealed class BricklinkTestHost : IDisposable
{
    internal const string BaseUrl = "https://api.bricklink.com/api/store/v1/";

    internal static readonly Uri BaseUri = new(BaseUrl);

    internal static readonly BricklinkCredentials DefaultCredentials = new()
    {
        ConsumerKey = "ck",
        ConsumerSecret = "cs",
        TokenValue = "tk",
        TokenSecret = "ts"
    };

    private readonly HttpClient _httpClient;

    private BricklinkTestHost(StubHttpMessageHandler handler, BricklinkRequestExecutor executor,
        HttpClient httpClient)
    {
        Handler = handler;
        Executor = executor;
        _httpClient = httpClient;
        Client = new BricklinkClient(executor, httpClient, disposeHttpClient: false);
    }

    internal StubHttpMessageHandler Handler { get; }

    internal BricklinkRequestExecutor Executor { get; }

    internal BricklinkClient Client { get; }

    internal StubHttpMessageHandler.CapturedRequest CapturedRequest => Handler.CapturedRequests.Single();

    internal static BricklinkTestHost Create(StubHttpMessageHandler handler,
        IRequestHandler? requestHandler = null, Func<BricklinkCredentials?>? credentialsProvider = null)
    {
        var httpClient = new HttpClient(handler);
        var options = new BricklinkRequestExecutorOptions(BaseUri, credentialsProvider ?? (() => DefaultCredentials),
            requestHandler, () => DateTimeOffset.FromUnixTimeSeconds(1700000000), () => "abc123");
        return new BricklinkTestHost(handler, new BricklinkRequestExecutor(httpClient, options), httpClient);
    }

    internal static BricklinkTestHost Create(string responseBody) => Create(new StubHttpMessageHandler(responseBody));

    public void Dispose()
    {
        Client.Dispose();
        _httpClient.Dispose();
    }
}
