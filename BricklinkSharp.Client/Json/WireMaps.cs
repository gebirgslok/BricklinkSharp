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

namespace BricklinkSharp.Client.Json;

/// <summary>
/// One map per enum: the single place where its wire format is written down.
/// Irregular enums use literal strings so renaming a member cannot change the wire format.
/// </summary>
internal static class WireMaps
{
    internal static readonly WireMap<OrderStatus> OrderStatus = WireMap<OrderStatus>.ByNameUpper();

    internal static readonly WireMap<ItemType> ItemType = WireMap<ItemType>.OfIgnoringCaseAndUnderscores(
        (Client.ItemType.Minifig, "minifig"),
        (Client.ItemType.Part, "part"),
        (Client.ItemType.Set, "set"),
        (Client.ItemType.Book, "book"),
        (Client.ItemType.Gear, "gear"),
        (Client.ItemType.Catalog, "catalog"),
        (Client.ItemType.Instruction, "instruction"),
        (Client.ItemType.UnsortedLot, "unsorted_lot"),
        (Client.ItemType.OriginalBox, "original_box"));

    internal static readonly WireMap<Condition> Condition = WireMap<Condition>.Of(
        (Client.Condition.New, "N"),
        (Client.Condition.Used, "U"));

    internal static readonly WireMap<PriceGuideSource> PriceGuideSource = WireMap<PriceGuideSource>.ByNameLower();

    internal static readonly WireMap<Direction> Direction = WireMap<Direction>.ByNameLower();

    internal static readonly WireMap<InventoryStatus> InventoryStatus = WireMap<InventoryStatus>.Of(
        (Client.InventoryStatus.Available, "Y"),
        (Client.InventoryStatus.InStockRoomA, "S"),
        (Client.InventoryStatus.InStockRoomB, "B"),
        (Client.InventoryStatus.InStockRoomC, "C"),
        (Client.InventoryStatus.Unavailable, "N"),
        (Client.InventoryStatus.Reserved, "R"));

    internal static readonly WireMap<CouponStatus> CouponStatus = WireMap<CouponStatus>.Of(
        (Client.CouponStatus.Open, "O"),
        (Client.CouponStatus.Redeemed, "A"),
        (Client.CouponStatus.Declined, "D"),
        (Client.CouponStatus.Expired, "E"));

    internal static readonly WireMap<ShippingArea> ShippingArea = WireMap<ShippingArea>.Of(
        (Client.ShippingArea.Domestic, "D"),
        (Client.ShippingArea.International, "I"),
        (Client.ShippingArea.Both, "B"));

    internal static readonly WireMap<AppearsAs> AppearsAs = WireMap<AppearsAs>.Of(
        (Client.AppearsAs.Regular, "R"),
        (Client.AppearsAs.Alternate, "A"),
        (Client.AppearsAs.Counterpart, "C"),
        (Client.AppearsAs.Extra, "E"));

    internal static readonly WireMap<FeedbackTargetRole> FeedbackTargetRole = WireMap<FeedbackTargetRole>.Of(
        (Client.FeedbackTargetRole.Seller, "S"),
        (Client.FeedbackTargetRole.Buyer, "B"));

    internal static readonly WireMap<DiscountType> DiscountType = WireMap<DiscountType>.Of(
        (Client.DiscountType.Fixed, "F"),
        (Client.DiscountType.Percentage, "S"));

    internal static readonly WireMap<Completeness> Completeness = WireMap<Completeness>.Of(
        (Client.Completeness.Complete, "C"),
        (Client.Completeness.Incomplete, "B"),
        (Client.Completeness.Sealed, "S"));

    internal static readonly WireMap<CouponRestrictionType> CouponRestrictionType = WireMap<CouponRestrictionType>.Of(
        (Client.CouponRestrictionType.ApplyToSpecifiedItemType, "A"),
        (Client.CouponRestrictionType.ExcludeSpecifiedType, "E"));

    internal static readonly WireMap<PushNotificationEventType> PushNotificationEventType = WireMap<PushNotificationEventType>.ByName();

    internal static readonly WireMap<Rating> Rating = WireMap<Rating>.ByName();

    internal static readonly WireMap<PaymentStatus> PaymentStatus = WireMap<PaymentStatus>.ByName();

    internal static readonly WireMap<VatOption> VatOption = WireMap<VatOption>.Of(
        (Client.VatOption.Exclude, "N"),
        (Client.VatOption.Include, "Y"),
        (Client.VatOption.IncludeAsNorway, "O"));
}
