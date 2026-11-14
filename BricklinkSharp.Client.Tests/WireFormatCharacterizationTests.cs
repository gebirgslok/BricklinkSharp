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
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Extensions;
using BricklinkSharp.Client.Json;
using BricklinkSharp.Client.Tests.Support;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

/// <summary>
/// Pins today's wire format for every enum and scalar: <c>ToDomainString</c> and each converter's
/// <c>Read</c>/<c>Write</c> for known values.
/// </summary>
public class WireFormatCharacterizationTests
{
    private static void AssertDomainStrings<TEnum>(Func<TEnum, string> toDomainString,
        params (TEnum Value, string Wire)[] expected) where TEnum : struct, Enum
    {
        ClassicAssert.AreEqual(Enum.GetValues(typeof(TEnum)).Length, expected.Length,
            $"the table must list every {typeof(TEnum).Name} member");

        foreach (var (value, wire) in expected)
        {
            ClassicAssert.AreEqual(wire, toDomainString(value), $"{typeof(TEnum).Name}.{value}");
        }
    }

    private static void AssertConverter<T>(JsonConverter<T> converter, params (T Value, string Wire)[] expected)
    {
        var enumType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        ClassicAssert.AreEqual(Enum.GetValues(enumType).Length, expected.Length,
            $"the table must list every {enumType.Name} member");

        foreach (var (value, wire) in expected)
        {
            ClassicAssert.AreEqual(value, ConverterHarness.Read(converter, ConverterHarness.Quote(wire)),
                $"read {wire}");
            ClassicAssert.AreEqual(ConverterHarness.Quote(wire), ConverterHarness.Write(converter, value),
                $"write {value}");
        }
    }

    [Test]
    public void Should_MapToWire_When_OrderStatusToDomainString()
    {
        AssertDomainStrings<OrderStatus>(e => e.ToDomainString(),
            (OrderStatus.Pending, "PENDING"), (OrderStatus.Updated, "UPDATED"), (OrderStatus.Processing, "PROCESSING"),
            (OrderStatus.Ready, "READY"), (OrderStatus.Paid, "PAID"), (OrderStatus.Packed, "PACKED"),
            (OrderStatus.Shipped, "SHIPPED"), (OrderStatus.Received, "RECEIVED"), (OrderStatus.Completed, "COMPLETED"),
            (OrderStatus.Ocr, "OCR"), (OrderStatus.Npb, "NPB"), (OrderStatus.Npx, "NPX"), (OrderStatus.Nrs, "NRS"),
            (OrderStatus.Nss, "NSS"), (OrderStatus.Cancelled, "CANCELLED"), (OrderStatus.Purged, "PURGED"));
    }

    [Test]
    public void Should_MapToWire_When_ItemTypeToDomainString()
    {
        AssertDomainStrings<ItemType>(e => e.ToDomainString(),
            (ItemType.Minifig, "minifig"), (ItemType.Part, "part"), (ItemType.Set, "set"), (ItemType.Book, "book"),
            (ItemType.Gear, "gear"), (ItemType.Catalog, "catalog"), (ItemType.Instruction, "instruction"),
            (ItemType.UnsortedLot, "unsorted_lot"), (ItemType.OriginalBox, "original_box"));
    }

    [Test]
    public void Should_MapToWire_When_ConditionToDomainString()
    {
        AssertDomainStrings<Condition>(e => e.ToDomainString(), (Condition.New, "N"), (Condition.Used, "U"));
    }

    [Test]
    public void Should_MapToWire_When_PriceGuideTypeToDomainString()
    {
        AssertDomainStrings<PriceGuideType>(e => e.ToDomainString(),
            (PriceGuideType.Sold, "sold"), (PriceGuideType.Stock, "stock"));
    }

    [Test]
    public void Should_MapToWire_When_DirectionsToDomainString()
    {
        AssertDomainStrings<Direction>(e => e.ToDomainString(), (Direction.In, "in"), (Direction.Out, "out"));
    }

    [Test]
    public void Should_MapToWire_When_InventoryStatusTypeToDomainString()
    {
        AssertDomainStrings<InventoryStatusType>(e => e.ToDomainString(),
            (InventoryStatusType.Available, "Y"), (InventoryStatusType.InStockRoomA, "S"),
            (InventoryStatusType.InStockRoomB, "B"), (InventoryStatusType.InStockRoomC, "C"),
            (InventoryStatusType.Unavailable, "N"), (InventoryStatusType.Reserved, "R"));
    }

    [Test]
    public void Should_MapToWire_When_CouponStatusToDomainString()
    {
        AssertDomainStrings<CouponStatus>(e => e.ToDomainString(),
            (CouponStatus.Open, "O"), (CouponStatus.Redeemed, "A"), (CouponStatus.Declined, "D"),
            (CouponStatus.Expired, "E"));
    }

    [Test]
    public void Should_MapToWire_When_ShippingAreaToDomainString()
    {
        AssertDomainStrings<ShippingArea>(e => e.ToDomainString(),
            (ShippingArea.Domestic, "D"), (ShippingArea.International, "I"), (ShippingArea.Both, "B"));
    }

    [Test]
    public void Should_MapToWire_When_AppearsAsToDomainString()
    {
        AssertDomainStrings<AppearsAs>(e => e.ToDomainString(),
            (AppearsAs.Regular, "R"), (AppearsAs.Alternate, "A"), (AppearsAs.Counterpart, "C"), (AppearsAs.Extra, "E"));
    }

    [Test]
    public void Should_MapToWire_When_RatingTargetRoleToDomainString()
    {
        AssertDomainStrings<RatingTargetRole>(e => e.ToDomainString(),
            (RatingTargetRole.Seller, "S"), (RatingTargetRole.Buyer, "B"));
    }

    [Test]
    public void Should_MapToWire_When_DiscountTypeToDomainString()
    {
        AssertDomainStrings<DiscountType>(e => e.ToDomainString(),
            (DiscountType.Fixed, "F"), (DiscountType.Percentage, "S"));
    }

    [Test]
    public void Should_MapToWire_When_CompletenessToDomainString()
    {
        AssertDomainStrings<Completeness>(e => e.ToDomainString(),
            (Completeness.Complete, "C"), (Completeness.Incomplete, "B"), (Completeness.Sealed, "S"));
    }

    [Test]
    public void Should_MapToWire_When_CouponRestrictionTypeToDomainString()
    {
        AssertDomainStrings<CouponRestrictionType>(e => e.ToDomainString(),
            (CouponRestrictionType.ApplyToSpecifiedItemType, "A"), (CouponRestrictionType.ExcludeSpecifiedType, "E"));
    }

    [Test]
    public void Should_MapToWire_When_EventTypeToDomainString()
    {
        AssertDomainStrings<EventType>(e => e.ToDomainString(),
            (EventType.Order, "Order"), (EventType.Message, "Message"), (EventType.Feedback, "Feedback"));
    }

    [Test]
    public void Should_MapToWire_When_RatingTypeToDomainString()
    {
        AssertDomainStrings<RatingType>(e => e.ToDomainString(),
            (RatingType.Praise, "Praise"), (RatingType.Neutral, "Neutral"), (RatingType.Complaint, "Complaint"));
    }

    [Test]
    public void Should_MapToWire_When_PaymentStatusToDomainString()
    {
        AssertDomainStrings<PaymentStatus>(e => e.ToDomainString(),
            (PaymentStatus.None, "None"), (PaymentStatus.Sent, "Sent"), (PaymentStatus.Received, "Received"),
            (PaymentStatus.Clearing, "Clearing"), (PaymentStatus.Returned, "Returned"),
            (PaymentStatus.Bounced, "Bounced"), (PaymentStatus.Completed, "Completed"));
    }

    [Test]
    public void Should_MapToWire_When_VatOptionToDomainString()
    {
        AssertDomainStrings<VatOption>(e => e.ToDomainString(),
            (VatOption.Exclude, "N"), (VatOption.Include, "Y"), (VatOption.IncludeAsNorway, "O"));
    }

    [Test]
    public void Should_RoundTrip_When_ConditionConverterIsUsed()
    {
        AssertConverter(new ConditionStringConverter(), (Condition.New, "N"), (Condition.Used, "U"));
    }

    [Test]
    public void Should_RoundTrip_When_CompletenessConverterIsUsed()
    {
        AssertConverter(new CompletenessStringConverter(),
            (Completeness.Complete, "C"), (Completeness.Incomplete, "B"), (Completeness.Sealed, "S"));
    }

    [Test]
    public void Should_RoundTrip_When_ShippingAreaConverterIsUsed()
    {
        AssertConverter(new ShippingAreaStringConverter(),
            (ShippingArea.Domestic, "D"), (ShippingArea.International, "I"), (ShippingArea.Both, "B"));
    }

    [Test]
    public void Should_RoundTrip_When_CouponStatusConverterIsUsed()
    {
        AssertConverter(new CouponStatusStringConverter(),
            (CouponStatus.Open, "O"), (CouponStatus.Redeemed, "A"), (CouponStatus.Declined, "D"),
            (CouponStatus.Expired, "E"));
    }

    [Test]
    public void Should_RoundTrip_When_AppearsAsConverterIsUsed()
    {
        AssertConverter(new AppearsAsStringConverter(),
            (AppearsAs.Regular, "R"), (AppearsAs.Alternate, "A"), (AppearsAs.Counterpart, "C"), (AppearsAs.Extra, "E"));
    }

    [Test]
    public void Should_RoundTrip_When_RatingTargetRoleConverterIsUsed()
    {
        AssertConverter(new RatingTargetRoleStringConverter(),
            (RatingTargetRole.Seller, "S"), (RatingTargetRole.Buyer, "B"));
    }

    [Test]
    public void Should_RoundTrip_When_DiscountTypeConverterIsUsed()
    {
        AssertConverter(new DiscountTypeStringConverter(), (DiscountType.Fixed, "F"), (DiscountType.Percentage, "S"));
    }

    [Test]
    public void Should_RoundTrip_When_OrderStatusConverterIsUsed()
    {
        AssertConverter(new OrderStatusStringConverter(),
            (OrderStatus.Pending, "PENDING"), (OrderStatus.Updated, "UPDATED"), (OrderStatus.Processing, "PROCESSING"),
            (OrderStatus.Ready, "READY"), (OrderStatus.Paid, "PAID"), (OrderStatus.Packed, "PACKED"),
            (OrderStatus.Shipped, "SHIPPED"), (OrderStatus.Received, "RECEIVED"), (OrderStatus.Completed, "COMPLETED"),
            (OrderStatus.Ocr, "OCR"), (OrderStatus.Npb, "NPB"), (OrderStatus.Npx, "NPX"), (OrderStatus.Nrs, "NRS"),
            (OrderStatus.Nss, "NSS"), (OrderStatus.Cancelled, "CANCELLED"), (OrderStatus.Purged, "PURGED"));
    }

    [Test]
    public void Should_RoundTrip_When_PaymentStatusConverterIsUsed()
    {
        AssertConverter(new PaymentStatusStringConverter(),
            (PaymentStatus.None, "None"), (PaymentStatus.Sent, "Sent"), (PaymentStatus.Received, "Received"),
            (PaymentStatus.Clearing, "Clearing"), (PaymentStatus.Returned, "Returned"),
            (PaymentStatus.Bounced, "Bounced"), (PaymentStatus.Completed, "Completed"));
    }

    [Test]
    public void Should_RoundTrip_When_EventTypeConverterIsUsed()
    {
        AssertConverter(new EventTypeStringConverter(),
            (EventType.Order, "Order"), (EventType.Message, "Message"), (EventType.Feedback, "Feedback"));
    }

    [Test]
    public void Should_RoundTripUpperCaseWire_When_ItemTypeConverterIsUsed()
    {
        AssertConverter(new ItemTypeStringConverter(),
            (ItemType.Minifig, "MINIFIG"), (ItemType.Part, "PART"), (ItemType.Set, "SET"), (ItemType.Book, "BOOK"),
            (ItemType.Gear, "GEAR"), (ItemType.Catalog, "CATALOG"), (ItemType.Instruction, "INSTRUCTION"),
            (ItemType.UnsortedLot, "UNSORTED_LOT"), (ItemType.OriginalBox, "ORIGINAL_BOX"));
    }

    [Test]
    public void Should_ReadCaseAndSeparatorVariants_When_ItemTypeConverterIsUsed()
    {
        var converter = new ItemTypeStringConverter();

        ClassicAssert.AreEqual(ItemType.Part, ConverterHarness.Read(converter, "\"part\""));
        ClassicAssert.AreEqual(ItemType.UnsortedLot, ConverterHarness.Read(converter, "\"unsorted_lot\""));
        ClassicAssert.AreEqual(ItemType.OriginalBox, ConverterHarness.Read(converter, "\"OriginalBox\""));
    }

    [Test]
    public void Should_RoundTripUpperCaseWire_When_NullableItemTypeConverterIsUsed()
    {
        AssertConverter(new NullableItemTypeStringConverter(),
            (ItemType.Minifig, "MINIFIG"), (ItemType.Part, "PART"), (ItemType.Set, "SET"), (ItemType.Book, "BOOK"),
            (ItemType.Gear, "GEAR"), (ItemType.Catalog, "CATALOG"), (ItemType.Instruction, "INSTRUCTION"),
            (ItemType.UnsortedLot, "UNSORTED_LOT"), (ItemType.OriginalBox, "ORIGINAL_BOX"));
    }

    [Test]
    public void Should_ReadNull_When_NullableItemTypeIsEmptyString()
    {
        ClassicAssert.IsNull(ConverterHarness.Read(new NullableItemTypeStringConverter(), "\"\""));
    }

    [Test]
    public void Should_RoundTrip_When_NullableCouponRestrictionTypeConverterIsUsed()
    {
        AssertConverter(new NullableCouponRestrictionTypeStringConverter(),
            (CouponRestrictionType.ApplyToSpecifiedItemType, "A"), (CouponRestrictionType.ExcludeSpecifiedType, "E"));
    }

    [Test]
    public void Should_ReadNull_When_NullableCouponRestrictionTypeIsEmptyString()
    {
        ClassicAssert.IsNull(ConverterHarness.Read(new NullableCouponRestrictionTypeStringConverter(), "\"\""));
    }

    [Test]
    public void Should_RoundTrip_When_RatingTypeConverterIsUsed()
    {
        var converter = new RatingTypeIntConverter();

        ClassicAssert.AreEqual(RatingType.Praise, ConverterHarness.Read(converter, "0"));
        ClassicAssert.AreEqual(RatingType.Neutral, ConverterHarness.Read(converter, "1"));
        ClassicAssert.AreEqual(RatingType.Complaint, ConverterHarness.Read(converter, "2"));
        ClassicAssert.AreEqual("0", ConverterHarness.Write(converter, RatingType.Praise));
        ClassicAssert.AreEqual("1", ConverterHarness.Write(converter, RatingType.Neutral));
        ClassicAssert.AreEqual("2", ConverterHarness.Write(converter, RatingType.Complaint));
    }

    [Test]
    public void Should_ReadAndWriteInvariantCulture_When_DecimalConverterIsUsed()
    {
        var converter = new DecimalStringConverter();

        ClassicAssert.AreEqual(1234.5m, ConverterHarness.Read(converter, "\"1234.5\""));
        ClassicAssert.AreEqual(0.1500m, ConverterHarness.Read(converter, "\"0.1500\""));
        ClassicAssert.AreEqual(-3m, ConverterHarness.Read(converter, "\"-3\""));
        ClassicAssert.AreEqual("\"1234.5\"", ConverterHarness.Write(converter, 1234.5m));
    }

    [Test]
    public void Should_ReadAndWriteInvariantCulture_When_DoubleConverterIsUsed()
    {
        var converter = new DoubleStringConverter();

        ClassicAssert.AreEqual(2.35, ConverterHarness.Read(converter, "\"2.35\""));
        ClassicAssert.AreEqual(-3.0, ConverterHarness.Read(converter, "\"-3\""));
        ClassicAssert.AreEqual("\"2.35\"", ConverterHarness.Write(converter, 2.35));
    }

    [Test]
    public void Should_RoundTripSignedQuantity_When_ChangedQuantityConverterIsUsed()
    {
        var converter = new ChangedQuantityStringConverter();

        ClassicAssert.AreEqual(5, ConverterHarness.Read(converter, "\"+5\""));
        ClassicAssert.AreEqual(-5, ConverterHarness.Read(converter, "\"-5\""));
        // System.Text.Json escapes '+' as + by default; it parses back to the same string
        ClassicAssert.AreEqual("\"\\u002B5\"", ConverterHarness.Write(converter, 5));
        ClassicAssert.AreEqual("\"\\u002B0\"", ConverterHarness.Write(converter, 0));
        ClassicAssert.AreEqual("\"-5\"", ConverterHarness.Write(converter, -5));
    }
}
