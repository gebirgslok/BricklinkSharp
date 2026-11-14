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
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BricklinkSharp.Client.OAuth;

namespace BricklinkSharp.Client;

internal sealed class BricklinkRequestExecutor
{
    private static readonly JsonSerializerOptions BodySerializerOptions = new()
    {
#if HAVE_JSON_DEFAULT_IGNORE_CONDITION
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
#else
        IgnoreNullValues = true,
#endif
    };

    private readonly HttpClient _httpClient;
    private readonly BricklinkRequestExecutorOptions _options;

    public BricklinkRequestExecutor(HttpClient httpClient, BricklinkRequestExecutorOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<T> SendAsync<T>(BricklinkRequest request, CancellationToken cancellationToken)
    {
        var credentials = ResolveCredentials(request);

        if (_options.RequestHandler != null)
        {
            await _options.RequestHandler
                .BeforeRequestAsync(request.Resource, request.Verb, credentials, cancellationToken)
                .ConfigureAwait(false);
        }

        var shape = BricklinkResponseParser.ResolveShape<T>(request.Shape);
        var expectedMetaCode = request.ExpectedMetaCode ?? GetDefaultExpectedMetaCode(request.Verb, shape);
        var httpMethod = ToHttpMethod(request.Verb);
        var url = request.BuildUrl(_options.BaseUri);

        using var message = BuildMessage(request, httpMethod, url, credentials);
        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif

        return BricklinkResponseParser.Parse<T>(responseBody, (int)response.StatusCode, expectedMetaCode, shape,
            url, httpMethod);
    }

    private BricklinkCredentials ResolveCredentials(BricklinkRequest request)
    {
        var credentials = request.Credentials ?? _options.DefaultCredentialsProvider();

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

    private HttpRequestMessage BuildMessage(BricklinkRequest request, HttpMethod httpMethod, string url,
        BricklinkCredentials credentials)
    {
        // everything that can throw runs before the message exists, so there is nothing to dispose on failure
        var signer = new OAuthRequest(credentials.ConsumerKey!, credentials.ConsumerSecret!,
            credentials.TokenValue!, credentials.TokenSecret!, url, httpMethod.Method,
            _options.Clock, _options.NonceGenerator);
        var header = signer.GetAuthorizationHeader().Split(' ');
        var json = request.Body == null ? null : JsonSerializer.Serialize(request.Body, BodySerializerOptions);

        var message = new HttpRequestMessage(httpMethod, url);
        message.Headers.Authorization = new AuthenticationHeaderValue(header[0], header[1]);

        if (json != null)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType!.CharSet = string.Empty;
            message.Content = content;
        }

        return message;
    }

    private static int GetDefaultExpectedMetaCode(HttpVerb verb, ResponseShape shape)
    {
        return verb switch
        {
            HttpVerb.Post => 201,
            HttpVerb.Put => shape == ResponseShape.NoData ? 204 : 200,
            HttpVerb.Delete => 204,
            _ => 200
        };
    }

    private static HttpMethod ToHttpMethod(HttpVerb verb)
    {
        return verb switch
        {
            HttpVerb.Get => HttpMethod.Get,
            HttpVerb.Post => HttpMethod.Post,
            HttpVerb.Put => HttpMethod.Put,
            HttpVerb.Delete => HttpMethod.Delete,
            HttpVerb.Patch => new HttpMethod("PATCH"),
            _ => throw new ArgumentOutOfRangeException(nameof(verb), verb, null)
        };
    }
}
