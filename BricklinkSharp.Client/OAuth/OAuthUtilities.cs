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
using System.Security.Cryptography;
using System.Text;

namespace BricklinkSharp.Client.OAuth
{
    internal static class OAuthUtilities
    {
        private static readonly DateTime _jan011970 = new DateTime(1970, 1, 1);
        private static readonly object _randomLock = new object();
        private static readonly Random _random = new Random();

        private static string ConstructRequestUrl(Uri url)
        {
            var builder = new StringBuilder();
            var requestUrl = $"{url.Scheme}://{url.Host}";
            var qualified = $":{url.Port}";
            var isBasic = url.Scheme == "http" && url.Port == 80;
            var isSecure = url.Scheme == "https" && url.Port == 443;
            builder.Append(requestUrl);
            builder.Append(!isBasic && !isSecure ? qualified : "");
            builder.Append(url.AbsolutePath);
            return builder.ToString();
        }

        private static string GetKey(string consumerSecret, string tokenSecret)
        {
            var builder = new StringBuilder();
            builder.Append(Uri.EscapeDataString(consumerSecret));
            builder.Append("&");
            builder.Append(Uri.EscapeDataString(tokenSecret));
            return builder.ToString();
        }

        private static string GetHmacSha1Hash(string signatureBaseString, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var baseStringBytes = Encoding.UTF8.GetBytes(signatureBaseString);
            using var sha1 = new HMACSHA1(keyBytes);
            var hash = sha1.ComputeHash(baseStringBytes);
            var base64 = Convert.ToBase64String(hash);
            return base64;
        }

        private static string Concatenate(List<WebParameter> collection, string separator, string spacer)
        {
            var builder = new StringBuilder();

            var total = collection.Count;
            var count = 0;

            foreach (var item in collection)
            {
                builder.Append(item.Name);
                builder.Append(separator);
                builder.Append(UrlEncodeStrict(item.Value));

                count++;

                if (count < total)
                {
                    builder.Append(spacer);
                }
            }

            return builder.ToString();
        }

        private static string NormalizeRequestParameters(List<WebParameter> parameters)
        {
            parameters.Sort();
            var concatenated = Concatenate(parameters, "=", "&");
            return concatenated;
        }

        private static string PercentEncode(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            var builder = new StringBuilder();

            foreach (var b in bytes)
            {
                if (b > 7 && b < 11 || b == 13)
                {
                    builder.Append($"%0{b:X}");
                }
                else
                {
                    builder.Append($"%{b:X}");
                }
            }
            return builder.ToString();
        }

        private static string UrlEncodeStrict(string value)
        {
            const string unreserved = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890-._~";
            var original = value;
            var ret = original.Where(c => !unreserved.Contains(c) && c != '%')
                .Aggregate(value, (current, c) => current.Replace(c.ToString(), PercentEncode(c.ToString())));
            return ret.Replace("%%", "%25%");
        }

        internal static string GetSignature(string signatureBase, string consumerSecret, string tokenSecret)
        {
            var key = GetKey(consumerSecret, tokenSecret);
            var signature = GetHmacSha1Hash(signatureBase, key);
            return Uri.EscapeDataString(signature);
        }

        internal static string ConcatenateRequestElements(string method, string url, List<WebParameter> parameters)
        {
            var builder = new StringBuilder();
            var requestMethod = string.Concat(method.ToUpperInvariant(), "&");
            var uri = new Uri(url);
            var requestUrl = string.Concat(Uri.EscapeDataString(ConstructRequestUrl(uri)), "&");
            parameters.AddRange(WebUtils.ParseQueryString(uri));
            var normalizedRequestParams = NormalizeRequestParameters(parameters);
            var requestParameters = Uri.EscapeDataString(normalizedRequestParams);
            builder.Append(requestMethod);
            builder.Append(requestUrl);
            builder.Append(requestParameters);
            return builder.ToString();
        }

        internal static string GetNonce()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";

            var nonce = new char[16];
            lock (_randomLock)
            {
                for (var i = 0; i < nonce.Length; i++)
                {
                    nonce[i] = chars[_random.Next(0, chars.Length)];
                }
            }
            return new string(nonce);
        }

        internal static string GetTimestamp()
        {
            var timeSpan = DateTime.UtcNow - _jan011970;
            var timestamp = (long)timeSpan.TotalSeconds;
            return timestamp.ToString();
        }
    }
}
