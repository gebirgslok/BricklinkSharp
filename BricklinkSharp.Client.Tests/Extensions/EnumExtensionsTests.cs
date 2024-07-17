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

using BricklinkSharp.Client.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests.Extensions;

public class EnumExtensionsTests
{
    [Test]
    public void ToDomainString_ItemType()
    {
        ClassicAssert.AreEqual("minifig", ItemType.Minifig.ToDomainString());
        ClassicAssert.AreEqual("part", ItemType.Part.ToDomainString());
        ClassicAssert.AreEqual("book", ItemType.Book.ToDomainString());
        ClassicAssert.AreEqual("catalog", ItemType.Catalog.ToDomainString());
        ClassicAssert.AreEqual("gear", ItemType.Gear.ToDomainString());
        ClassicAssert.AreEqual("instruction", ItemType.Instruction.ToDomainString());
        ClassicAssert.AreEqual("original_box", ItemType.OriginalBox.ToDomainString());
        ClassicAssert.AreEqual("unsorted_lot", ItemType.UnsortedLot.ToDomainString());
        ItemType? itemType = null;
        ClassicAssert.IsNull(itemType?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PartOutItemType()
    {
        ClassicAssert.AreEqual("M", PartOutItemType.Minifig.ToDomainString());
        ClassicAssert.AreEqual("G", PartOutItemType.Gear.ToDomainString());
        ClassicAssert.AreEqual("S", PartOutItemType.Set.ToDomainString());

        PartOutItemType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_OrderStatus()
    {
        ClassicAssert.AreEqual("PENDING", OrderStatus.Pending.ToDomainString());
        ClassicAssert.AreEqual("UPDATED", OrderStatus.Updated.ToDomainString());
        ClassicAssert.AreEqual("PROCESSING", OrderStatus.Processing.ToDomainString());
        ClassicAssert.AreEqual("READY", OrderStatus.Ready.ToDomainString());
        ClassicAssert.AreEqual("PAID", OrderStatus.Paid.ToDomainString());
        ClassicAssert.AreEqual("PACKED", OrderStatus.Packed.ToDomainString());
        ClassicAssert.AreEqual("SHIPPED", OrderStatus.Shipped.ToDomainString());
        ClassicAssert.AreEqual("RECEIVED", OrderStatus.Received.ToDomainString());
        ClassicAssert.AreEqual("COMPLETED", OrderStatus.Completed.ToDomainString());
        ClassicAssert.AreEqual("OCR", OrderStatus.Ocr.ToDomainString());
        ClassicAssert.AreEqual("NPB", OrderStatus.Npb.ToDomainString());
        ClassicAssert.AreEqual("NPX", OrderStatus.Npx.ToDomainString());
        ClassicAssert.AreEqual("NRS", OrderStatus.Nrs.ToDomainString());
        ClassicAssert.AreEqual("NSS", OrderStatus.Nss.ToDomainString());
        ClassicAssert.AreEqual("CANCELLED", OrderStatus.Cancelled.ToDomainString());
        ClassicAssert.AreEqual("PURGED", OrderStatus.Purged.ToDomainString());

        OrderStatus? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Condition()
    {
        ClassicAssert.AreEqual("N", Condition.New.ToDomainString());
        ClassicAssert.AreEqual("U", Condition.Used.ToDomainString());

        Condition? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PriceGuideType()
    {
        ClassicAssert.AreEqual("sold", PriceGuideType.Sold.ToDomainString());
        ClassicAssert.AreEqual("stock", PriceGuideType.Stock.ToDomainString());

        PriceGuideType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_OrderDirection()
    {
        ClassicAssert.AreEqual("in", OrderDirection.In.ToDomainString());
        ClassicAssert.AreEqual("out", OrderDirection.Out.ToDomainString());

        OrderDirection? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Direction()
    {
        ClassicAssert.AreEqual("in", Direction.In.ToDomainString());
        ClassicAssert.AreEqual("out", Direction.Out.ToDomainString());

        Direction? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_InventoryStatusType()
    {
        ClassicAssert.AreEqual("Y", InventoryStatusType.Available.ToDomainString());
        ClassicAssert.AreEqual("S", InventoryStatusType.InStockRoomA.ToDomainString());
        ClassicAssert.AreEqual("B", InventoryStatusType.InStockRoomB.ToDomainString());
        ClassicAssert.AreEqual("C", InventoryStatusType.InStockRoomC.ToDomainString());
        ClassicAssert.AreEqual("N", InventoryStatusType.Unavailable.ToDomainString());
        ClassicAssert.AreEqual("R", InventoryStatusType.Reserved.ToDomainString());

        InventoryStatusType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_CouponStatus()
    {
        ClassicAssert.AreEqual("O", CouponStatus.Open.ToDomainString());
        ClassicAssert.AreEqual("A", CouponStatus.Redeemed.ToDomainString());
        ClassicAssert.AreEqual("D", CouponStatus.Declined.ToDomainString());
        ClassicAssert.AreEqual("E", CouponStatus.Expired.ToDomainString());

        CouponStatus? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_ShippingArea()
    {
        ClassicAssert.AreEqual("D", ShippingArea.Domestic.ToDomainString());
        ClassicAssert.AreEqual("I", ShippingArea.International.ToDomainString());
        ClassicAssert.AreEqual("B", ShippingArea.Both.ToDomainString());

        ShippingArea? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_AppearsAs()
    {
        ClassicAssert.AreEqual("R", AppearsAs.Regular.ToDomainString());
        ClassicAssert.AreEqual("A", AppearsAs.Alternate.ToDomainString());
        ClassicAssert.AreEqual("C", AppearsAs.Counterpart.ToDomainString());
        ClassicAssert.AreEqual("E", AppearsAs.Extra.ToDomainString());

        AppearsAs? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_RatingTargetRole()
    {
        ClassicAssert.AreEqual("S", RatingTargetRole.Seller.ToDomainString());
        ClassicAssert.AreEqual("B", RatingTargetRole.Buyer.ToDomainString());

        RatingTargetRole? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_DiscountType()
    {
        ClassicAssert.AreEqual("F", DiscountType.Fixed.ToDomainString());
        ClassicAssert.AreEqual("S", DiscountType.Percentage.ToDomainString());

        DiscountType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Completeness()
    {
        ClassicAssert.AreEqual("C", Completeness.Complete.ToDomainString());
        ClassicAssert.AreEqual("B", Completeness.Incomplete.ToDomainString());
        ClassicAssert.AreEqual("S", Completeness.Sealed.ToDomainString());

        Completeness? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_CouponRestrictionType()
    {
        ClassicAssert.AreEqual("A", CouponRestrictionType.ApplyToSpecifiedItemType.ToDomainString());
        ClassicAssert.AreEqual("E", CouponRestrictionType.ExcludeSpecifiedType.ToDomainString());

        CouponRestrictionType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_EventType()
    {
        ClassicAssert.AreEqual("Order", EventType.Order.ToDomainString());
        ClassicAssert.AreEqual("Message", EventType.Message.ToDomainString());
        ClassicAssert.AreEqual("Feedback", EventType.Feedback.ToDomainString());

        EventType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_RatingType()
    {
        ClassicAssert.AreEqual("Complaint", RatingType.Complaint.ToDomainString());
        ClassicAssert.AreEqual("Neutral", RatingType.Neutral.ToDomainString());
        ClassicAssert.AreEqual("Praise", RatingType.Praise.ToDomainString());

        RatingType? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PaymentStatus()
    {
        ClassicAssert.AreEqual("Bounced", PaymentStatus.Bounced.ToDomainString());
        ClassicAssert.AreEqual("Clearing", PaymentStatus.Clearing.ToDomainString());
        ClassicAssert.AreEqual("Completed", PaymentStatus.Completed.ToDomainString());
        ClassicAssert.AreEqual("None", PaymentStatus.None.ToDomainString());
        ClassicAssert.AreEqual("Received", PaymentStatus.Received.ToDomainString());
        ClassicAssert.AreEqual("Returned", PaymentStatus.Returned.ToDomainString());
        ClassicAssert.AreEqual("Sent", PaymentStatus.Sent.ToDomainString());

        PaymentStatus? nullHolder = null;
        ClassicAssert.IsNull(nullHolder?.ToDomainString());
    }
}