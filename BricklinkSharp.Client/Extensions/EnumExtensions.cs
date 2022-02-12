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

namespace BricklinkSharp.Client.Extensions;

internal static class EnumExtensions
{
    internal static string ToDomainString(this OrderStatus e)
    {
        return e switch
        {
            OrderStatus.Pending => nameof(OrderStatus.Pending).ToUpper(),
            OrderStatus.Updated => nameof(OrderStatus.Updated).ToUpper(),
            OrderStatus.Processing => nameof(OrderStatus.Processing).ToUpper(),
            OrderStatus.Ready => nameof(OrderStatus.Ready).ToUpper(),
            OrderStatus.Paid => nameof(OrderStatus.Paid).ToUpper(),
            OrderStatus.Packed => nameof(OrderStatus.Packed).ToUpper(),
            OrderStatus.Shipped => nameof(OrderStatus.Shipped).ToUpper(),
            OrderStatus.Received => nameof(OrderStatus.Received).ToUpper(),
            OrderStatus.Completed => nameof(OrderStatus.Completed).ToUpper(),
            OrderStatus.Ocr => nameof(OrderStatus.Ocr).ToUpper(),
            OrderStatus.Npb => nameof(OrderStatus.Npb).ToUpper(),
            OrderStatus.Npx => nameof(OrderStatus.Npx).ToUpper(),
            OrderStatus.Nrs => nameof(OrderStatus.Nrs).ToUpper(),
            OrderStatus.Nss => nameof(OrderStatus.Nss).ToUpper(),
            OrderStatus.Cancelled => nameof(OrderStatus.Cancelled).ToUpper(),
            OrderStatus.Purged => nameof(OrderStatus.Purged).ToUpper(),
            _ => throw new ArgumentOutOfRangeException(nameof(OrderStatus), e, null)
        };
    }

    internal static string ToDomainString(this PartOutItemType e)
    {
        return e switch
        {
            PartOutItemType.Set => "S",
            PartOutItemType.Minifig => "M",
            PartOutItemType.Gear => "G",
            _ => throw new ArgumentOutOfRangeException(nameof(PartOutItemType), e, null)
        };
    }

    internal static string ToDomainString(this ItemType e)
    {
        return e switch
        {
            ItemType.Minifig => nameof(ItemType.Minifig).ToLowerInvariant(),
            ItemType.Part => nameof(ItemType.Part).ToLowerInvariant(),
            ItemType.Set => nameof(ItemType.Set).ToLowerInvariant(),
            ItemType.Book => nameof(ItemType.Book).ToLowerInvariant(),
            ItemType.Gear => nameof(ItemType.Gear).ToLowerInvariant(),
            ItemType.Catalog => nameof(ItemType.Catalog).ToLowerInvariant(),
            ItemType.Instruction => nameof(ItemType.Instruction).ToLowerInvariant(),
            ItemType.UnsortedLot => "unsorted_lot",
            ItemType.OriginalBox => "original_box",
            _ => throw new ArgumentOutOfRangeException(nameof(ItemType), e, null)
        };
    }

    internal static string ToDomainString(this Condition e)
    {
        return e switch
        {
            Condition.New => "N",
            Condition.Used => "U",
            _ => throw new ArgumentOutOfRangeException(nameof(Condition), e, null)
        };
    }

    internal static string ToDomainString(this PriceGuideType e)
    {
        return e switch
        {
            PriceGuideType.Sold => nameof(PriceGuideType.Sold).ToLowerInvariant(),
            PriceGuideType.Stock => nameof(PriceGuideType.Stock).ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(PriceGuideType), e, null)
        };
    }

    internal static string ToDomainString(this OrderDirection e)
    {
        return e switch
        {
            OrderDirection.In => nameof(OrderDirection.In).ToLowerInvariant(),
            OrderDirection.Out => nameof(OrderDirection.Out).ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(OrderDirection), e, null)
        };
    }

    internal static string ToDomainString(this Direction e)
    {
        return e switch
        {
            Direction.In => nameof(Direction.In).ToLowerInvariant(),
            Direction.Out => nameof(Direction.Out).ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(Direction), e, null)
        };
    }

    internal static string ToDomainString(this InventoryStatusType e)
    {
        return e switch
        {
            InventoryStatusType.Available => "Y",
            InventoryStatusType.InStockRoomA => "S",
            InventoryStatusType.InStockRoomB => "B",
            InventoryStatusType.InStockRoomC => "C",
            InventoryStatusType.Unavailable => "N",
            InventoryStatusType.Reserved => "R",
            _ => throw new ArgumentOutOfRangeException(nameof(InventoryStatusType), e, null)
        };
    }

    internal static string ToDomainString(this CouponStatus e)
    {
        return e switch
        {
            CouponStatus.Open => "O",
            CouponStatus.Redeemed => "A",
            CouponStatus.Declined => "D",
            CouponStatus.Expired => "E",
            _ => throw new ArgumentOutOfRangeException(nameof(CouponStatus), e, null)
        };
    }

    internal static string ToDomainString(this ShippingArea e)
    {
        return e switch
        {
            ShippingArea.Domestic => "D",
            ShippingArea.International => "I",
            ShippingArea.Both => "B",
            _ => throw new ArgumentOutOfRangeException(nameof(ShippingArea), e, null)
        };
    }

    internal static string ToDomainString(this AppearsAs e)
    {
        return e switch
        {
            AppearsAs.Regular => "R",
            AppearsAs.Alternate => "A",
            AppearsAs.Counterpart => "C",
            AppearsAs.Extra => "E",
            _ => throw new ArgumentOutOfRangeException(nameof(AppearsAs), e, null)
        };
    }

    internal static string ToDomainString(this RatingTargetRole e)
    {
        return e switch
        {
            RatingTargetRole.Seller => "S",
            RatingTargetRole.Buyer => "B",
            _ => throw new ArgumentOutOfRangeException(nameof(RatingTargetRole), e, null)
        };
    }

    internal static string ToDomainString(this DiscountType e)
    {
        return e switch
        {
            DiscountType.Fixed => "F",
            DiscountType.Percentage => "S",
            _ => throw new ArgumentOutOfRangeException(nameof(DiscountType), e, null)
        };
    }

    internal static string ToDomainString(this Completeness e)
    {
        return e switch
        {
            Completeness.Complete => "C",
            Completeness.Incomplete => "B",
            Completeness.Sealed => "S",
            _ => throw new ArgumentOutOfRangeException(nameof(Completeness), e, null)
        };
    }

    internal static string ToDomainString(this CouponRestrictionType e)
    {
        return e switch
        {
            CouponRestrictionType.ApplyToSpecifiedItemType => "A",
            CouponRestrictionType.ExcludeSpecifiedType => "E",
            _ => throw new ArgumentOutOfRangeException(nameof(CouponRestrictionType), e, null)
        };
    }

    internal static string ToDomainString(this EventType e)
    {
        return e switch
        {
            EventType.Order => nameof(EventType.Order),
            EventType.Message => nameof(EventType.Message),
            EventType.Feedback => nameof(EventType.Feedback),
            _ => throw new ArgumentOutOfRangeException(nameof(EventType), e, null)
        };
    }

    internal static string ToDomainString(this RatingType e)
    {
        return e switch
        {
            RatingType.Praise => nameof(RatingType.Praise),
            RatingType.Neutral => nameof(RatingType.Neutral),
            RatingType.Complaint => nameof(RatingType.Complaint),
            _ => throw new ArgumentOutOfRangeException(nameof(RatingType), e, null)
        };
    }

    internal static string ToDomainString(this PaymentStatus e)
    {
        return e switch
        {
            PaymentStatus.Bounced => nameof(PaymentStatus.Bounced),
            PaymentStatus.Clearing => nameof(PaymentStatus.Clearing),
            PaymentStatus.Completed => nameof(PaymentStatus.Completed),
            PaymentStatus.None => nameof(PaymentStatus.None),
            PaymentStatus.Received => nameof(PaymentStatus.Received),
            PaymentStatus.Returned => nameof(PaymentStatus.Returned),
            PaymentStatus.Sent => nameof(PaymentStatus.Sent),
            _ => throw new ArgumentOutOfRangeException(nameof(PaymentStatus), e, null)
        };
    }
}