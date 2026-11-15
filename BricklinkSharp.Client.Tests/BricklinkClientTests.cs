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
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class BricklinkClientTests
{
    [TestCase("//img.bricklink.com/ItemImage/PN/34/43898pb006.png", "https")]
    [TestCase("//img.bricklink.com/ItemImage/PN/34/43898pb006.png", "http")]
    public void EnsureImageUrlScheme(string url, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.EnsureImageUrlScheme(url, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/PN/34/43898pb006.png", uri.AbsoluteUri);
    }

    // URL shapes are covered offline in ImageUrlsTests; this is the one check that BrickLink still serves them.
    [Test]
    public async Task Should_BeServedByBrickLink_When_ImageUrlsAreBuilt()
    {
        using var client = BricklinkClientFactory.Build();
        using var http = new HttpClient();

        foreach (var uri in new[] { client.GetSetImage("6090-1"), client.GetPartImageForColor("2540", 10) })
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, uri);
            using var response = await http.SendAsync(request);

            ClassicAssert.IsTrue(response.IsSuccessStatusCode, uri.ToString());
        }
    }
}
