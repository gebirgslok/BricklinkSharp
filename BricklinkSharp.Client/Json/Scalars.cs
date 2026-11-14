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
using System.Text.Json;

namespace BricklinkSharp.Client.Json;

/// <summary>Strict parsing of numeric wire values; malformed input throws <see cref="JsonException"/>.</summary>
internal static class Scalars
{
    private const NumberStyles PlainNumber = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    internal static decimal ParseDecimal(string? wire)
    {
        if (decimal.TryParse(wire, PlainNumber, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw Malformed(wire, nameof(Decimal));
    }

    internal static double ParseDouble(string? wire)
    {
        if (double.TryParse(wire, PlainNumber, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw Malformed(wire, nameof(Double));
    }

    internal static Rating ParseRating(int wire)
    {
        if (Enum.IsDefined(typeof(Rating), wire))
        {
            return (Rating)wire;
        }

        throw Malformed(wire.ToString(CultureInfo.InvariantCulture), nameof(Rating));
    }

    private static JsonException Malformed(string? wire, string target) =>
        new($"'{wire}' is not a valid {target}.");
}
