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

using BricklinkSharp.Client.Json;

namespace BricklinkSharp.Client.Extensions;

internal static class EnumExtensions
{
    internal static string ToDomainString(this OrderStatus e) => WireMaps.OrderStatus.ToWire(e);

    internal static string ToDomainString(this ItemType e) => WireMaps.ItemType.ToWire(e);

    internal static string ToDomainString(this Condition e) => WireMaps.Condition.ToWire(e);

    internal static string ToDomainString(this PriceGuideType e) => WireMaps.PriceGuideType.ToWire(e);

    internal static string ToDomainString(this OrderDirection e) => WireMaps.OrderDirection.ToWire(e);

    internal static string ToDomainString(this Direction e) => WireMaps.Direction.ToWire(e);

    internal static string ToDomainString(this InventoryStatusType e) => WireMaps.InventoryStatusType.ToWire(e);

    internal static string ToDomainString(this CouponStatus e) => WireMaps.CouponStatus.ToWire(e);

    internal static string ToDomainString(this ShippingArea e) => WireMaps.ShippingArea.ToWire(e);

    internal static string ToDomainString(this AppearsAs e) => WireMaps.AppearsAs.ToWire(e);

    internal static string ToDomainString(this RatingTargetRole e) => WireMaps.RatingTargetRole.ToWire(e);

    internal static string ToDomainString(this DiscountType e) => WireMaps.DiscountType.ToWire(e);

    internal static string ToDomainString(this Completeness e) => WireMaps.Completeness.ToWire(e);

    internal static string ToDomainString(this CouponRestrictionType e) => WireMaps.CouponRestrictionType.ToWire(e);

    internal static string ToDomainString(this EventType e) => WireMaps.EventType.ToWire(e);

    internal static string ToDomainString(this RatingType e) => WireMaps.RatingType.ToWire(e);

    internal static string ToDomainString(this PaymentStatus e) => WireMaps.PaymentStatus.ToWire(e);

    internal static string ToDomainString(this VatOption e) => WireMaps.VatOption.ToWire(e);
}
