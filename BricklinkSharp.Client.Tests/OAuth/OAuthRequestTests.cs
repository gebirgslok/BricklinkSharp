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
using BricklinkSharp.Client.OAuth;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests.OAuth;

public class OAuthRequestTests
{
    private const string Url = "https://api.bricklink.com/api/store/v1/orders?direction=in";

    private static OAuthRequest CreateRequest() => new OAuthRequest("ck", "cs", "tk", "ts", Url, "get",
        () => DateTimeOffset.FromUnixTimeSeconds(1700000000), () => "abc123");

    [Test]
    public void Should_ProduceKnownHeader_When_ClockAndNonceAreFixed()
    {
        var header = CreateRequest().GetAuthorizationHeader();

        ClassicAssert.AreEqual(
            "OAuth oauth_consumer_key=\"ck\",oauth_nonce=\"abc123\"," +
            "oauth_signature=\"Mt9DcFtkNprCaD%2FEBV7cyWOjnko%3D\"," +
            "oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1700000000\"," +
            "oauth_token=\"tk\",oauth_version=\"1.0\"",
            header);
    }

    [Test]
    public void Should_ProduceKnownBaseString_When_ConcatenatingRequestElements()
    {
        var parameters = new List<WebParameter>
        {
            new WebParameter("oauth_version", "1.0"),
            new WebParameter("oauth_token", "tk"),
            new WebParameter("oauth_timestamp", "1700000000"),
            new WebParameter("oauth_signature_method", "HMAC-SHA1"),
            new WebParameter("oauth_nonce", "abc123"),
            new WebParameter("oauth_consumer_key", "ck")
        };

        var baseString = OAuthUtilities.ConcatenateRequestElements("get", Url, parameters);

        ClassicAssert.AreEqual(
            "GET&https%3A%2F%2Fapi.bricklink.com%2Fapi%2Fstore%2Fv1%2Forders&" +
            "direction%3Din%26oauth_consumer_key%3Dck%26oauth_nonce%3Dabc123%26" +
            "oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D1700000000%26" +
            "oauth_token%3Dtk%26oauth_version%3D1.0",
            baseString);
    }

    [Test]
    public void Should_NotMutateCallerParameters_When_Signing()
    {
        var parameters = new List<WebParameter> { new WebParameter("b", "2"), new WebParameter("a", "1") };

        CreateRequest().GetAuthorizationHeader(parameters);
        OAuthUtilities.ConcatenateRequestElements("GET", Url, parameters);

        CollectionAssert.AreEqual(new[] { "b=2", "a=1" }, parameters.ConvertAll(p => p.ToString()));
    }

    [Test]
    public void Should_ProduceSameHeader_When_SignedTwice()
    {
        var request = CreateRequest();

        ClassicAssert.AreEqual(request.GetAuthorizationHeader(), request.GetAuthorizationHeader());
    }
}
