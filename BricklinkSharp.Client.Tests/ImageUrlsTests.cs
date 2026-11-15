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
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class ImageUrlsTests
{
    [TestCase(ItemType.Minifig, "sw1093", "//img.bricklink.com/ItemImage/MN/0/sw1093.png", "//img.bricklink.com/M/sw1093.jpg")]
    [TestCase(ItemType.Set, "6090-1", "//img.bricklink.com/ItemImage/SN/0/6090-1.png", "//img.bricklink.com/S/6090-1.jpg")]
    [TestCase(ItemType.Book, "5002772", "//img.bricklink.com/ItemImage/BN/0/5002772.png", "//img.bricklink.com/B/5002772.jpg")]
    [TestCase(ItemType.Gear, "GMRacer1", "//img.bricklink.com/ItemImage/GN/0/GMRacer1.png", "//img.bricklink.com/G/GMRacer1.jpg")]
    [TestCase(ItemType.Catalog, "x1", "//img.bricklink.com/ItemImage/CN/0/x1.png", "//img.bricklink.com/C/x1.jpg")]
    [TestCase(ItemType.Instruction, "723-1", "//img.bricklink.com/ItemImage/IN/0/723-1.png", "//img.bricklink.com/I/723-1.jpg")]
    [TestCase(ItemType.OriginalBox, "723-1", "//img.bricklink.com/ItemImage/ON/0/723-1.png", "//img.bricklink.com/ON/723-1.jpg")]
    public void Should_BuildImageAndThumbnailUrls_When_ItemTypeHasNoColor(ItemType type, string itemNumber, string image,
        string thumbnail)
    {
        ClassicAssert.AreEqual($"https:{image}", ImageUrls.Image(type, itemNumber, null, "https")!.AbsoluteUri);
        ClassicAssert.AreEqual($"http:{thumbnail}", ImageUrls.Thumbnail(type, itemNumber, null, "http")!.AbsoluteUri);
    }

    [Test]
    public void Should_PutColorBeforeItemNumber_When_ItemTypeIsPart()
    {
        ClassicAssert.AreEqual("https://img.bricklink.com/ItemImage/PN/34/43898pb006.png",
            ImageUrls.Image(ItemType.Part, "43898pb006", 34, "https")!.AbsoluteUri);
        ClassicAssert.AreEqual("https://img.bricklink.com/P/34/43898pb006.jpg",
            ImageUrls.Thumbnail(ItemType.Part, "43898pb006", 34, "https")!.AbsoluteUri);
    }

    [Test]
    public void Should_ReturnNull_When_PartHasNoColor()
    {
        ClassicAssert.IsNull(ImageUrls.Image(ItemType.Part, "3001", null, "https"));
        ClassicAssert.IsNull(ImageUrls.Thumbnail(ItemType.Part, "3001", null, "https"));
    }

    [Test]
    public void Should_ReturnNull_When_ItemTypeHasNoImageUrl()
    {
        ClassicAssert.IsNull(ImageUrls.Image(ItemType.UnsortedLot, "x", null, "https"));
        ClassicAssert.IsNull(ImageUrls.Thumbnail(ItemType.UnsortedLot, "x", null, "https"));
    }

    [Test]
    public void Should_EscapeItemNumber_When_ItNeedsEncoding()
    {
        var uri = ImageUrls.Image(ItemType.Set, "a/b?c", null, "https")!;

        ClassicAssert.AreEqual("https://img.bricklink.com/ItemImage/SN/0/a%2Fb%3Fc.png", uri.AbsoluteUri);
    }

    [Test]
    public void Should_HaveUrlForEveryItemTypeExceptUnsortedLot_When_ColorIsGiven()
    {
        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            var expectNull = type == ItemType.UnsortedLot;

            ClassicAssert.AreEqual(expectNull, ImageUrls.Image(type, "1", 1, "https") == null, type.ToString());
            ClassicAssert.AreEqual(expectNull, ImageUrls.Thumbnail(type, "1", 1, "https") == null, type.ToString());
        }
    }

    [Test]
    public void Should_UseClientDispatchers_When_CallingGetImageAndGetThumbnail()
    {
        using var client = BricklinkClientFactory.Build();

        ClassicAssert.AreEqual("https://img.bricklink.com/ItemImage/SN/0/6090-1.png",
            client.GetImage("6090-1", ItemType.Set)!.AbsoluteUri);
        ClassicAssert.AreEqual("https://img.bricklink.com/P/5/3001.jpg",
            client.GetThumbnail("3001", ItemType.Part, 5)!.AbsoluteUri);
        ClassicAssert.IsNull(client.GetImage("3001", ItemType.Part));
    }
}
