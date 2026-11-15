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
using BricklinkSharp.Client.OAuth;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests.OAuth;

public class OAuth1SignerTests
{
    private static readonly Uri Resource = new Uri("https://api.bricklink.com/api/store/v1/orders");
    private static readonly DateTimeOffset Timestamp = DateTimeOffset.FromUnixTimeSeconds(1700000000);

    private static readonly BricklinkCredentials Credentials = new BricklinkCredentials
    {
        ConsumerKey = "ck",
        ConsumerSecret = "cs",
        TokenValue = "tk",
        TokenSecret = "ts"
    };

    private static string Header(QueryString query) =>
        OAuth1Signer.Sign(HttpMethod.Get, Resource, query, Credentials, Timestamp, "abc123").ToString();

    [Test]
    public void Should_ProduceKnownHeader_When_ClockAndNonceAreFixed()
    {
        var header = Header(default(QueryString).With("direction", "in"));

        ClassicAssert.AreEqual(
            "OAuth oauth_consumer_key=\"ck\",oauth_nonce=\"abc123\"," +
            "oauth_signature=\"Mt9DcFtkNprCaD%2FEBV7cyWOjnko%3D\"," +
            "oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1700000000\"," +
            "oauth_token=\"tk\",oauth_version=\"1.0\"",
            header);
    }

    [Test]
    public void Should_UseOAuthScheme_When_Signing()
    {
        var header = OAuth1Signer.Sign(HttpMethod.Get, Resource, default, Credentials, Timestamp, "abc123");

        ClassicAssert.AreEqual("OAuth", header.Scheme);
        StringAssert.StartsWith("oauth_consumer_key=\"ck\",", header.Parameter!);
    }

    [Test]
    public void Should_ProduceKnownBaseString_When_PairsAreUnsorted()
    {
        var pairs = new[]
        {
            new KeyValuePair<string, string>("oauth_version", "1.0"),
            new KeyValuePair<string, string>("direction", "in"),
            new KeyValuePair<string, string>("oauth_consumer_key", "ck")
        };

        var baseString = OAuth1Signer.BuildBaseString(HttpMethod.Get, Resource, pairs);

        ClassicAssert.AreEqual(
            "GET&https%3A%2F%2Fapi.bricklink.com%2Fapi%2Fstore%2Fv1%2Forders&" +
            "direction%3Din%26oauth_consumer_key%3Dck%26oauth_version%3D1.0",
            baseString);
    }

    [Test]
    public void Should_EncodeHeaderValues_When_TokenHasReservedCharacters()
    {
        var credentials = new BricklinkCredentials
        {
            ConsumerKey = "ck",
            ConsumerSecret = "cs",
            TokenValue = "t+k/1",
            TokenSecret = "ts"
        };

        var header = OAuth1Signer.Sign(HttpMethod.Get, Resource, default, credentials, Timestamp, "abc123");

        StringAssert.Contains("oauth_token=\"t%2Bk%2F1\"", header.Parameter!);
    }

    [Test]
    public void Should_ProduceSameHeader_When_SignedTwice()
    {
        var query = default(QueryString).With("status", "a,b,-c");

        ClassicAssert.AreEqual(Header(query), Header(query));
    }

    [Test]
    public void Should_DropDefaultPortAndQuery_When_BuildingBaseString()
    {
        var withPort = new Uri("https://api.bricklink.com:443/api/store/v1/orders?x=1");

        var baseString = OAuth1Signer.BuildBaseString(HttpMethod.Get, withPort, new List<KeyValuePair<string, string>>());

        StringAssert.StartsWith("GET&https%3A%2F%2Fapi.bricklink.com%2Fapi%2Fstore%2Fv1%2Forders&", baseString);
    }

    [Test]
    public void Should_EncodeFilterCommasTwice_When_BuildingBaseString()
    {
        var baseString = OAuth1Signer.BuildBaseString(HttpMethod.Get, Resource,
            default(QueryString).With("status", "a,b,-c").Pairs);

        StringAssert.EndsWith("&status%3Da%252Cb%252C-c", baseString);
    }

    [Test]
    public void Should_EncodeNonAsciiAsUtf8Twice_When_BuildingBaseString()
    {
        var baseString = OAuth1Signer.BuildBaseString(HttpMethod.Get, Resource,
            default(QueryString).With("q", "é \U0001F9F1").Pairs);

        StringAssert.EndsWith("&q%3D%25C3%25A9%2520%25F0%259F%25A7%25B1", baseString);
    }

    // Remove with OAuthRequest in RFC 0006 phase 4.
    [TestCase("a,b,-c")]
    [TestCase("a+b")]
    [TestCase("a b")]
    [TestCase("x~y")]
    [TestCase("a&b=c")]
    public void Should_MatchLegacySignature_When_ValueIsAsciiWithoutPercent(string value)
    {
        var query = default(QueryString).With("status", value);
        var url = $"{Resource}?{query.Encode()}";
        var legacy = new OAuthRequest("ck", "cs", "tk", "ts", url, "GET", () => Timestamp, () => "abc123")
            .GetAuthorizationHeader();

        ClassicAssert.AreEqual(legacy, Header(query));
    }
}
