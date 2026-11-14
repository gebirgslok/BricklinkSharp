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
using System.Collections;
using System.Linq;
using System.Text.Json;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class WireMapTests
{
    private enum Sample
    {
        A,
        B
    }

    private static IEnumerable AllMaps()
    {
        yield return Case(WireMaps.OrderStatus);
        yield return Case(WireMaps.ItemType);
        yield return Case(WireMaps.Condition);
        yield return Case(WireMaps.PriceGuideType);
        yield return Case(WireMaps.OrderDirection);
        yield return Case(WireMaps.Direction);
        yield return Case(WireMaps.InventoryStatusType);
        yield return Case(WireMaps.CouponStatus);
        yield return Case(WireMaps.ShippingArea);
        yield return Case(WireMaps.AppearsAs);
        yield return Case(WireMaps.RatingTargetRole);
        yield return Case(WireMaps.DiscountType);
        yield return Case(WireMaps.Completeness);
        yield return Case(WireMaps.CouponRestrictionType);
        yield return Case(WireMaps.EventType);
        yield return Case(WireMaps.RatingType);
        yield return Case(WireMaps.PaymentStatus);
        yield return Case(WireMaps.VatOption);
    }

    private static TestCaseData Case<TEnum>(WireMap<TEnum> map) where TEnum : struct, Enum =>
        new TestCaseData((Action)(() => AssertRoundTrips(map))).SetName($"Should_RoundTripEveryMember_When_Map{typeof(TEnum).Name}");

    [TestCaseSource(nameof(AllMaps))]
    public void Should_RoundTripEveryMember_When_MapIsRegistered(Action assertion) => assertion();

    [Test]
    public void Should_Throw_When_ValueIsMappedTwice()
    {
        Assert.Throws<ArgumentException>(() => WireMap<Sample>.Of((Sample.A, "x"), (Sample.A, "y")));
    }

    [Test]
    public void Should_Throw_When_WireIsMappedTwice()
    {
        Assert.Throws<ArgumentException>(() => WireMap<Sample>.Of((Sample.A, "x"), (Sample.B, "x")));
    }

    [Test]
    public void Should_Throw_When_WireDiffersOnlyInCaseAndMapIgnoresCase()
    {
        Assert.Throws<ArgumentException>(() => WireMap<Sample>.OfIgnoringCase((Sample.A, "x"), (Sample.B, "X")));
    }

    [Test]
    public void Should_ThrowJsonException_When_StrictConverterReadsUnknownWire()
    {
        var converter = new WireStringConverter<Condition>(WireMaps.Condition);

        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"BOGUS\""));
    }

    [Test]
    public void Should_ThrowJsonException_When_StrictNullableConverterReadsUnknownWire()
    {
        var converter = new NullableWireStringConverter<Condition>(WireMaps.Condition);

        Assert.Throws<JsonException>(() => ConverterHarness.Read(converter, "\"BOGUS\""));
        ClassicAssert.IsNull(ConverterHarness.Read(converter, "\"\""));
    }

    [Test]
    public void Should_IgnoreCaseAndUnderscores_When_MapIsItemType()
    {
        ClassicAssert.AreEqual(ItemType.UnsortedLot, WireMaps.ItemType.FromWire("UNSORTEDLOT"));
        ClassicAssert.AreEqual(ItemType.UnsortedLot, WireMaps.ItemType.FromWire("Unsorted_Lot"));
    }

    [Test]
    public void Should_WriteUpperAndLowerCase_When_MapIsByNameUpperOrLower()
    {
        ClassicAssert.AreEqual("PENDING", WireMaps.OrderStatus.ToWire(OrderStatus.Pending));
        ClassicAssert.AreEqual("sold", WireMaps.PriceGuideType.ToWire(PriceGuideType.Sold));
        ClassicAssert.AreEqual("Praise", WireMaps.RatingType.ToWire(RatingType.Praise));
    }

    [Test]
    public void Should_ReadCaseInsensitively_When_MapIsOfIgnoringCase()
    {
        var map = WireMap<Sample>.OfIgnoringCase((Sample.A, "x"));

        ClassicAssert.AreEqual(Sample.A, map.FromWire("X"));
    }

    [Test]
    public void Should_ThrowFormatException_When_WireIsUnknown()
    {
        Assert.Throws<FormatException>(() => WireMaps.Condition.FromWire("X"));
        Assert.Throws<FormatException>(() => WireMaps.Condition.FromWire(null));
    }

    [Test]
    public void Should_ThrowArgumentOutOfRange_When_ValueIsUnmapped()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WireMaps.Condition.ToWire((Condition)99));
    }

    [Test]
    public void Should_ReadCaseSensitively_When_MapUsesLiterals()
    {
        ClassicAssert.IsFalse(WireMaps.Condition.TryFromWire("n", out _));
    }

    [Test]
    public void Should_ReadCaseInsensitively_When_MapIsByName()
    {
        ClassicAssert.AreEqual(OrderStatus.Pending, WireMaps.OrderStatus.FromWire("pending"));
    }

    private static void AssertRoundTrips<TEnum>(WireMap<TEnum> map) where TEnum : struct, Enum
    {
        var members = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

        ClassicAssert.IsNotEmpty(members);

        foreach (var member in members)
        {
            ClassicAssert.AreEqual(member, map.FromWire(map.ToWire(member)), $"{typeof(TEnum).Name}.{member}");
        }
    }
}
