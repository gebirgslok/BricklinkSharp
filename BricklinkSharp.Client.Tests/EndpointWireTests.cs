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
using System.Threading.Tasks;
using BricklinkSharp.Client.OAuth;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointWireTests
{
    private static readonly Uri BaseUri = new("https://api.bricklink.com/api/store/v1/");

    private static readonly EndpointWire.Signing FixedSigning = new(
        new BricklinkCredentials { ConsumerKey = "ck", ConsumerSecret = "cs", TokenValue = "tk", TokenSecret = "ts" },
        () => DateTimeOffset.FromUnixTimeSeconds(1700000000), () => "abc123");

    private static Endpoint<NoData> NewEndpoint(string path = "items/part/3001") =>
        Endpoint<NoData>.Get(ApiResourceType.Item, path);

    private static string BuildUrl<T>(Endpoint<T> endpoint)
    {
        using var request = EndpointWire.ToRequest(endpoint, BaseUri, FixedSigning);
        return request.RequestUri!.AbsoluteUri;
    }

    [Test]
    public void Should_JoinBaseAndPathWithSingleSlash_When_PathHasLeadingSlash()
    {
        var url = BuildUrl(NewEndpoint("/items/part/3001"));

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001", url);
    }

    [Test]
    public void Should_SkipQueryParameter_When_ValueIsNull()
    {
        var url = BuildUrl(NewEndpoint().WithQuery("color_id", null).WithQuery("guide_type", "sold"));

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001?guide_type=sold", url);
    }

    [Test]
    public void Should_EscapeQueryNameAndValue_When_TheyContainReservedCharacters()
    {
        var url = BuildUrl(NewEndpoint().WithQuery("status", "pending,paid").WithQuery("a b", "x&y"));

        StringAssert.EndsWith("?status=pending%2Cpaid&a%20b=x%26y", url);
    }

    [Test]
    public void Should_FormatNumbersWithInvariantCulture_When_CurrentCultureUsesCommaDecimals()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");

        try
        {
            var url = BuildUrl(NewEndpoint().WithQuery("price", 1.5m).WithQuery("flag", true));

            StringAssert.EndsWith("?price=1.5&flag=true", url);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void Should_Throw_When_QueryValueIsCollection()
    {
        Assert.Throws<ArgumentException>(() => NewEndpoint().WithQuery("ids", new List<int> { 1, 2 }));
    }

    [Test]
    public void Should_Throw_When_QueryValueIsEnum()
    {
        Assert.Throws<ArgumentException>(() => NewEndpoint().WithQuery("type", HttpVerb.Get));
    }

    [Test]
    public void Should_FormatDatesAsIso8601_When_QueryValueIsDate()
    {
        var url = BuildUrl(NewEndpoint().WithQuery("since", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)));

        StringAssert.Contains("since=2026-01-02T03%3A04%3A05.0000000Z", url);
    }

    [Test]
    public void Should_JoinValuesWithCommas_When_QueryValueIsList()
    {
        var url = BuildUrl(NewEndpoint().WithQuery("status", new[] { "a", "b", "-c" }));

        StringAssert.EndsWith("?status=a%2Cb%2C-c", url);
    }

    [Test]
    public void Should_DropQueryParameter_When_ListIsNullOrEmpty()
    {
        var url = BuildUrl(NewEndpoint().WithQuery("status", (IEnumerable<string>?)null)
            .WithQuery("item_type", Array.Empty<string>()));

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001", url);
    }

    [Test]
    public void Should_ProduceExpectedAuthorizationHeader_When_ClockAndNonceAreFixed()
    {
        using var request = EndpointWire.ToRequest(
            Endpoint<NoData>.Get(ApiResourceType.Order, "orders").WithQuery("direction", "in"), BaseUri, FixedSigning);

        ClassicAssert.AreEqual("OAuth", request.Headers.Authorization!.Scheme);
        StringAssert.Contains("oauth_nonce=\"abc123\"", request.Headers.Authorization.Parameter!);
        StringAssert.Contains("oauth_timestamp=\"1700000000\"", request.Headers.Authorization.Parameter!);
        StringAssert.Contains("oauth_consumer_key=\"ck\"", request.Headers.Authorization.Parameter!);
    }

    [Test]
    public void Should_SignTheQueryItSends_When_QueryHasIncludeExcludeFilter()
    {
        var endpoint = NewEndpoint().WithQuery("status", new[] { "a", "b", "-c" }).WithQuery("a b", "x+y~z");

        using var request = EndpointWire.ToRequest(endpoint, BaseUri, FixedSigning);

        var sentPairs = request.RequestUri!.Query.TrimStart('?').Split('&').Select(pair => pair.Split('='))
            .Aggregate(default(QueryString),
                (query, pair) => query.With(Uri.UnescapeDataString(pair[0]), Uri.UnescapeDataString(pair[1])));
        var expected = OAuth1Signer.Sign(HttpMethod.Get, new Uri("https://api.bricklink.com/api/store/v1/items/part/3001"),
            sentPairs, FixedSigning.Credentials, FixedSigning.Clock(), FixedSigning.Nonce());
        ClassicAssert.AreEqual(expected.ToString(), request.Headers.Authorization!.ToString());
        StringAssert.EndsWith("?status=a%2Cb%2C-c&a%20b=x%2By~z", request.RequestUri!.AbsoluteUri);
    }

    [Test]
    public async Task Should_SendJsonBodyWithoutCharset_When_EndpointHasBody()
    {
        var endpoint = Endpoint<NoData>.Put(ApiResourceType.OrderStatus, "orders/1/status")
            .WithBody(new FieldChange { Field = "status", Value = "PAID" });

        using var request = EndpointWire.ToRequest(endpoint, BaseUri, FixedSigning);

        ClassicAssert.AreEqual(HttpMethod.Put, request.Method);
        ClassicAssert.AreEqual("application/json", request.Content!.Headers.ContentType!.ToString());
        ClassicAssert.AreEqual("{\"field\":\"status\",\"value\":\"PAID\"}", await request.Content.ReadAsStringAsync());
    }
}
