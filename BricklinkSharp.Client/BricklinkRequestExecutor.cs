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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BricklinkSharp.Client.OAuth;

namespace BricklinkSharp.Client;

internal sealed class BricklinkRequestExecutor : IEndpointSender
{
    private readonly HttpClient _httpClient;
    private readonly BricklinkRequestExecutorOptions _options;

    public BricklinkRequestExecutor(HttpClient httpClient, BricklinkRequestExecutorOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<T> SendAsync<T>(Endpoint<T> endpoint, BricklinkCredentials? callCredentials,
        CancellationToken cancellationToken)
    {
        if (endpoint.Rejection != null)
        {
            throw endpoint.Rejection;
        }

        var credentials = ResolveCredentials(callCredentials);

        if (_options.RequestHandler != null)
        {
            await _options.RequestHandler
                .BeforeRequestAsync(endpoint.Resource, endpoint.Verb, credentials, cancellationToken)
                .ConfigureAwait(false);
        }

        var signing = new EndpointWire.Signing(credentials, _options.Clock ?? (() => DateTimeOffset.UtcNow),
            _options.NonceGenerator ?? OAuthUtilities.GetNonce);

        using var message = EndpointWire.ToRequest(endpoint, _options.BaseUri, signing);
        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif

        return EndpointWire.ReadResponse(endpoint, message.RequestUri!, message.Method, (int)response.StatusCode,
            responseBody);
    }

    private BricklinkCredentials ResolveCredentials(BricklinkCredentials? callCredentials)
    {
        var credentials = callCredentials ?? _options.DefaultCredentialsProvider();

        if (credentials == null)
        {
            throw new BricklinkMissingCredentialsException(new List<string>
            {
                nameof(BricklinkCredentials.ConsumerKey),
                nameof(BricklinkCredentials.ConsumerSecret),
                nameof(BricklinkCredentials.TokenValue),
                nameof(BricklinkCredentials.TokenSecret)
            });
        }

        credentials.ValidateThrowException();
        return credentials;
    }
}
