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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BricklinkSharp.Client.Tests;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseBody;
    private readonly HttpStatusCode _statusCode;

    public StubHttpMessageHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responseBody = responseBody;
        _statusCode = statusCode;
    }

    public List<CapturedRequest> CapturedRequests { get; } = new();

    public DisposalTrackingContent? ResponseContent { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content == null ? null : await request.Content.ReadAsStringAsync();
        CapturedRequests.Add(new CapturedRequest(request.Method, request.RequestUri!.AbsoluteUri,
            request.Headers.Authorization?.ToString(), body, request.Content?.Headers.ContentType?.ToString()));

        ResponseContent = new DisposalTrackingContent(_responseBody);
        return new HttpResponseMessage(_statusCode) { Content = ResponseContent };
    }

    internal sealed record CapturedRequest(HttpMethod Method, string Url, string? Authorization, string? Body,
        string? ContentType);

    internal sealed class DisposalTrackingContent : StringContent
    {
        public DisposalTrackingContent(string content) : base(content, Encoding.UTF8, "application/json")
        {
        }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
