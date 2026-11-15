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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace BricklinkSharp.Client.OAuth;

internal static class OAuth1Signer
{
    private const string SignatureMethod = "HMAC-SHA1";
    private const string Version = "1.0";

    public static AuthenticationHeaderValue Sign(HttpMethod method, Uri baseResource, QueryString query,
        BricklinkCredentials credentials, DateTimeOffset timestamp, string nonce)
    {
        var timestampText = timestamp.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var oauthPairs = new[]
        {
            new KeyValuePair<string, string>("oauth_consumer_key", credentials.ConsumerKey!),
            new KeyValuePair<string, string>("oauth_token", credentials.TokenValue!),
            new KeyValuePair<string, string>("oauth_signature_method", SignatureMethod),
            new KeyValuePair<string, string>("oauth_timestamp", timestampText),
            new KeyValuePair<string, string>("oauth_nonce", nonce),
            new KeyValuePair<string, string>("oauth_version", Version)
        };

        var baseString = BuildBaseString(method, baseResource, query.Pairs.Concat(oauthPairs));
        var signature = Rfc3986.Encode(ComputeSignature(baseString, credentials));

        var parameter = $"oauth_consumer_key=\"{Rfc3986.Encode(credentials.ConsumerKey!)}\"," +
                        $"oauth_nonce=\"{Rfc3986.Encode(nonce)}\"," +
                        $"oauth_signature=\"{signature}\"," +
                        $"oauth_signature_method=\"{SignatureMethod}\"," +
                        $"oauth_timestamp=\"{timestampText}\"," +
                        $"oauth_token=\"{Rfc3986.Encode(credentials.TokenValue!)}\"," +
                        $"oauth_version=\"{Version}\"";
        return new AuthenticationHeaderValue("OAuth", parameter);
    }

    internal static string BuildBaseString(HttpMethod method, Uri baseResource,
        IEnumerable<KeyValuePair<string, string>> pairs)
    {
        var normalized = string.Join("&", pairs
            .Select(p => new KeyValuePair<string, string>(Rfc3986.Encode(p.Key), Rfc3986.Encode(p.Value)))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .ThenBy(p => p.Value, StringComparer.Ordinal)
            .Select(p => $"{p.Key}={p.Value}"));

        return $"{method.Method.ToUpperInvariant()}&{Rfc3986.Encode(NormalizeResource(baseResource))}&{Rfc3986.Encode(normalized)}";
    }

    private static string NormalizeResource(Uri uri)
    {
        var isDefaultPort = uri.Scheme == "http" && uri.Port == 80 || uri.Scheme == "https" && uri.Port == 443;
        var port = isDefaultPort ? "" : $":{uri.Port}";
        return $"{uri.Scheme}://{uri.Host}{port}{uri.AbsolutePath}";
    }

    private static string ComputeSignature(string baseString, BricklinkCredentials credentials)
    {
        var key = $"{Rfc3986.Encode(credentials.ConsumerSecret!)}&{Rfc3986.Encode(credentials.TokenSecret!)}";
        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString)));
    }
}
