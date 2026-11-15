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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class Rfc3986Tests
{
    [TestCase("AZaz09-._~", "AZaz09-._~")]
    [TestCase("a,b,-c", "a%2Cb%2C-c")]
    [TestCase("a+b", "a%2Bb")]
    [TestCase("a b", "a%20b")]
    [TestCase("&=%", "%26%3D%25")]
    [TestCase("", "")]
    public void Should_EncodeAsciiStrictly_When_ValueHasReservedCharacters(string value, string expected)
    {
        ClassicAssert.AreEqual(expected, Rfc3986.Encode(value));
    }

    [Test]
    public void Should_EncodeAsUtf8_When_ValueIsNonAscii()
    {
        ClassicAssert.AreEqual("%C3%A9%E2%82%AC", Rfc3986.Encode("é€"));
    }

    [Test]
    public void Should_EncodeSurrogatePairAsOneSequence_When_ValueIsOutsideBmp()
    {
        ClassicAssert.AreEqual("%F0%9F%A7%B1", Rfc3986.Encode("\U0001F9F1"));
    }

    [Test]
    public void Should_PadHexToTwoDigits_When_ByteIsBelow0x10()
    {
        ClassicAssert.AreEqual("%0B%0C%00", Rfc3986.Encode("\u000b\u000c\u0000"));
    }
}
