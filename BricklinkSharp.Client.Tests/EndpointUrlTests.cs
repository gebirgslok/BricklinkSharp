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
using System.Globalization;
using System.Net.Http;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointUrlTests
{
    private static readonly Uri BaseUri = new("https://api.bricklink.com/api/store/v1/");

    private static Endpoint<NoData> NewEndpoint(string path = "items/part/3001") =>
        Endpoint<NoData>.Get(ApiResourceType.Item, path);

    [Test]
    public void Should_JoinBaseAndPathWithSingleSlash_When_PathHasLeadingSlash()
    {
        var url = NewEndpoint("/items/part/3001").BuildUrl(BaseUri);

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001", url);
    }

    [Test]
    public void Should_SkipQueryParameter_When_ValueIsNull()
    {
        var url = NewEndpoint().WithQuery("color_id", null).WithQuery("guide_type", "sold").BuildUrl(BaseUri);

        ClassicAssert.AreEqual("https://api.bricklink.com/api/store/v1/items/part/3001?guide_type=sold", url);
    }

    [Test]
    public void Should_EscapeQueryNameAndValue_When_TheyContainReservedCharacters()
    {
        var url = NewEndpoint().WithQuery("status", "pending,paid").WithQuery("a b", "x&y").BuildUrl(BaseUri);

        StringAssert.EndsWith("?status=pending%2Cpaid&a%20b=x%26y", url);
    }

    [Test]
    public void Should_FormatNumbersWithInvariantCulture_When_CurrentCultureUsesCommaDecimals()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");

        try
        {
            var url = NewEndpoint().WithQuery("price", 1.5m).WithQuery("flag", true).BuildUrl(BaseUri);

            StringAssert.EndsWith("?price=1.5&flag=true", url);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Test]
    public void Should_Throw_When_QueryValueIsEnum()
    {
        Assert.Throws<ArgumentException>(() => NewEndpoint().WithQuery("type", HttpVerb.Get));
    }

    [Test]
    public void Should_FormatDatesAsIso8601_When_QueryValueIsDate()
    {
        var url = NewEndpoint().WithQuery("since", new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc))
            .BuildUrl(BaseUri);

        StringAssert.Contains("since=2026-01-02T03%3A04%3A05.0000000Z", url);
    }
}
