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
using System.Net.Http;
using System.Text;
using BricklinkSharp.Client.OAuth;

namespace BricklinkSharp.Client;

/// <summary>Turns an <see cref="Endpoint{T}"/> into a signed request. Pure: no I/O, time and nonce come from <see cref="Signing"/>.</summary>
internal static class EndpointWire
{
    internal sealed class Signing
    {
        public Signing(BricklinkCredentials credentials, Func<DateTimeOffset> clock, Func<string> nonce)
        {
            Credentials = credentials;
            Clock = clock;
            Nonce = nonce;
        }

        public BricklinkCredentials Credentials { get; }

        public Func<DateTimeOffset> Clock { get; }

        public Func<string> Nonce { get; }
    }

    internal static HttpRequestMessage ToRequest<T>(Endpoint<T> endpoint, Uri baseUri, Signing signing)
    {
        var method = ToHttpMethod(endpoint.Verb);
        var resource = new Uri($"{baseUri.AbsoluteUri.TrimEnd('/')}/{endpoint.Path.TrimStart('/')}");
        var query = endpoint.Query.Encode();
        var url = query.Length == 0 ? resource.AbsoluteUri : $"{resource.AbsoluteUri}?{query}";

        // everything that can throw runs before the message exists, so there is nothing to dispose on failure
        var authorization = OAuth1Signer.Sign(method, resource, endpoint.Query, signing.Credentials,
            signing.Clock(), signing.Nonce());
        var json = endpoint.Body == null ? null : WireJson.Serialize(endpoint.Body);

        var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = authorization;

        if (json != null)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType!.CharSet = string.Empty;
            message.Content = content;
        }

        return message;
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
