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
    private static async Task<bool> CheckUriExistsAsync(Uri uri)
    {
        try
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Head, uri);
            var message = await client.SendAsync(request);
            var data = await message.Content.ReadAsByteArrayAsync();
            return data.Length == 0;
        }
        catch
        {
            return false;
        }
    }

    [TestCase("//img.bricklink.com/ItemImage/PN/34/43898pb006.png", "https")]
    [TestCase("//img.bricklink.com/ItemImage/PN/34/43898pb006.png", "http")]
    public void EnsureImageUrlScheme(string url, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.EnsureImageUrlScheme(url, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/PN/34/43898pb006.png", uri.AbsoluteUri);
    }

    [TestCase("2540", 10, "https")]
    [TestCase("2540", 10, "http")]
    [TestCase("43898pb006", 34, "https")]
    public async Task GetPartImageForColorAsync(string partNo, int colorId, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetPartImageForColor(partNo, colorId, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/PN/{colorId}/{partNo}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("soc130", "https")]
    [TestCase("soc130", "http")]
    [TestCase("85863pb095", "https")]
    [TestCase("sw1093", "https")]
    public async Task GetMinifigImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetMinifigImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/MN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("723-1", "https")]
    [TestCase("723-1", "http")]
    [TestCase("7774-1", "https")]
    [TestCase("6090-1", "https")]
    public async Task GetSetImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetSetImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/SN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("DKPiratesNL", "https")]
    [TestCase("DKPiratesNL", "http")]
    [TestCase("5002772", "https")]
    [TestCase("b65de2", "http")]
    public async Task GetBookImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetBookImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/BN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("BioGMC041", "https")]
    [TestCase("BioGMC041", "http")]
    [TestCase("GMRacer1", "https")]
    [TestCase("flyermariode", "http")]
    public async Task GetGearImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetGearImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/GN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("c58dk2", "https")]
    [TestCase("c58dk2", "http")]
    [TestCase("c77de", "https")]
    [TestCase("c84uk", "http")]
    public async Task GetCatalogImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetCatalogImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/CN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("1518-1", "https")]
    [TestCase("1518-1", "http")]
    [TestCase("2996-1", "https")]
    [TestCase("4128-1", "http")]
    public async Task GetInstructionImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetInstructionImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/IN/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }

    [TestCase("2964-1", "https")]
    [TestCase("3552-1", "http")]
    [TestCase("3552-1", "https")]
    [TestCase("4709-1", "http")]
    public async Task GetOriginalBoxImage(string number, string scheme)
    {
        using var client = BricklinkClientFactory.Build();
        var uri = client.GetOriginalBoxImage(number, scheme);

        ClassicAssert.AreEqual($"{scheme}://img.bricklink.com/ItemImage/ON/0/{number}.png", uri.AbsoluteUri);
        ClassicAssert.IsTrue(await CheckUriExistsAsync(uri));
    }
}