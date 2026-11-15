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

namespace BricklinkSharp.Client;

internal static class ImageUrls
{
    private const string Host = "//img.bricklink.com";

    // {0} is the item number. Parts take the color ID first: {0} = color ID, {1} = item number.
    private static readonly Dictionary<ItemType, (string Image, string Thumbnail)> _paths = new()
    {
        [ItemType.Part] = ("ItemImage/PN/{0}/{1}.png", "P/{0}/{1}.jpg"),
        [ItemType.Minifig] = ("ItemImage/MN/0/{0}.png", "M/{0}.jpg"),
        [ItemType.Set] = ("ItemImage/SN/0/{0}.png", "S/{0}.jpg"),
        [ItemType.Book] = ("ItemImage/BN/0/{0}.png", "B/{0}.jpg"),
        [ItemType.Gear] = ("ItemImage/GN/0/{0}.png", "G/{0}.jpg"),
        [ItemType.Catalog] = ("ItemImage/CN/0/{0}.png", "C/{0}.jpg"),
        [ItemType.Instruction] = ("ItemImage/IN/0/{0}.png", "I/{0}.jpg"),
        [ItemType.OriginalBox] = ("ItemImage/ON/0/{0}.png", "ON/{0}.jpg")
    };

    // Null when the type has no image URL (unsorted lot) or a part is given without a color.
    public static Uri? Image(ItemType type, string itemNumber, int? colorId, string scheme) =>
        Build(type, itemNumber, colorId, scheme, paths => paths.Image);

    public static Uri? Thumbnail(ItemType type, string itemNumber, int? colorId, string scheme) =>
        Build(type, itemNumber, colorId, scheme, paths => paths.Thumbnail);

    public static Uri WithScheme(string imageUrl, string scheme) => new Uri($"{scheme}:{imageUrl}");

    private static Uri? Build(ItemType type, string itemNumber, int? colorId, string scheme,
        Func<(string Image, string Thumbnail), string> selectTemplate)
    {
        if (!_paths.TryGetValue(type, out var paths))
        {
            return null;
        }

        var escapedItemNumber = Uri.EscapeDataString(itemNumber);
        string path;

        if (type == ItemType.Part)
        {
            if (!colorId.HasValue)
            {
                return null;
            }

            path = string.Format(selectTemplate(paths), colorId.Value, escapedItemNumber);
        }
        else
        {
            path = string.Format(selectTemplate(paths), escapedItemNumber);
        }

        return WithScheme($"{Host}/{path}", scheme);
    }
}
