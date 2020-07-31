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
using System.Text;

namespace BricklinkSharp.Client.OAuth
{
    internal class OAuthRequest
    {
        public string ConsumerKey { get; }

        public string ConsumerSecret { get; }

        public string Token { get; }

        public string TokenSecret { get; }

        public string RequestUrl { get; }

        public string Method { get; }

        public OAuthRequest(string consumerKey, string consumerSecret, string token, string tokenSecret, string requestUrl, string method)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            Token = token;
            TokenSecret = tokenSecret;
            RequestUrl = requestUrl;
            Method = method;
        }

        private string BuildAuthHeader(string signature, string timestamp,
            string nonce)
        {
            var builder = new StringBuilder();
            builder.Append("OAuth ");
            builder.AppendFormat($"oauth_consumer_key=\"{ConsumerKey}\",");
            builder.AppendFormat($"oauth_nonce=\"{nonce}\",");
            builder.AppendFormat($"oauth_signature=\"{signature}\",");
            builder.AppendFormat("oauth_signature_method=\"HMAC-SHA1\",");
            builder.AppendFormat($"oauth_timestamp=\"{timestamp}\",");
            builder.AppendFormat($"oauth_token=\"{Token}\",");
            builder.AppendFormat("oauth_version=\"1.0\"");
            return builder.ToString();
        }

        private void AddAuthParamters(List<WebParameter> parameter, string timestamp, string nonce)
        {
            var authParameters = new List<WebParameter>
            {
                new WebParameter("oauth_consumer_key", ConsumerKey),
                new WebParameter("oauth_token", Token),
                new WebParameter("oauth_signature_method", "HMAC-SHA1"),
                new WebParameter("oauth_timestamp", timestamp),
                new WebParameter("oauth_nonce", nonce),
                new WebParameter("oauth_version", "1.0")
            };

            parameter.AddRange(authParameters);
        }

        private string GetSignature(string timestamp, string nonce, List<WebParameter> queryParameters = null)
        {
            var parameters = queryParameters ?? new List<WebParameter>();
            AddAuthParamters(parameters, timestamp, nonce);
            var signatureBase = OAuthUtilities.ConcatenateRequestElements(Method.ToUpperInvariant(), RequestUrl, parameters);
            var signature = OAuthUtilities.GetSignature(signatureBase, ConsumerSecret, TokenSecret);
            return signature;
        }

        public string GetAuthorizationHeader(List<WebParameter> queryParameters = null)
        {
            var timestamp = OAuthUtilities.GetTimestamp();
            var nonce = OAuthUtilities.GetNonce();
            var signature = GetSignature(timestamp, nonce, queryParameters);
            var header = BuildAuthHeader(signature, timestamp, nonce);
            return header;
        }
    }
}
