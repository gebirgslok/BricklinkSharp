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

using BricklinkSharp.Client.OAuth;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests.OAuth;

// Pins where the legacy OAuthUtilities.UrlEncodeStrict and Rfc3986.Encode agree and where they differ.
// Remove together with OAuthUtilities.UrlEncodeStrict in RFC 0006 phase 4.
public class EncodingCharacterizationTests
{
    [TestCase("")]
    [TestCase("abcXYZ019-._~")]
    [TestCase("a,b,-c")]
    [TestCase("a+b")]
    [TestCase("a b")]
    [TestCase("a&b=c")]
    [TestCase("a/b?c#d")]
    [TestCase("\t\n\r")]
    public void Should_AgreeWithLegacy_When_ValueIsAsciiWithoutPercent(string value)
    {
        ClassicAssert.AreEqual(OAuthUtilities.UrlEncodeStrict(value), Rfc3986.Encode(value));
    }

    [Test]
    public void Should_EncodePercent_When_LegacyLeftItUnencoded()
    {
        ClassicAssert.AreEqual("50%", OAuthUtilities.UrlEncodeStrict("50%"));
        ClassicAssert.AreEqual("50%25", Rfc3986.Encode("50%"));
    }

    [Test]
    public void Should_PadHex_When_LegacyEmittedSingleDigit()
    {
        ClassicAssert.AreEqual("%B", OAuthUtilities.UrlEncodeStrict("\u000b"));
        ClassicAssert.AreEqual("%0B", Rfc3986.Encode("\u000b"));
    }

    [Test]
    public void Should_EncodeSurrogatePairAsOneSequence_When_LegacySplitIt()
    {
        const string brick = "\U0001F9F1";

        ClassicAssert.AreEqual("%F0%9F%A7%B1", Rfc3986.Encode(brick));
        ClassicAssert.AreNotEqual(Rfc3986.Encode(brick), OAuthUtilities.UrlEncodeStrict(brick));
    }

    [Test]
    public void Should_AgreeWithLegacy_When_ValueIsNonAsciiBmpCharacter()
    {
        ClassicAssert.AreEqual(OAuthUtilities.UrlEncodeStrict("é"), Rfc3986.Encode("é"));
    }
}
