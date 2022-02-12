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

namespace BricklinkSharp.Client.Tests.Extensions;

public class EnumExtensionsTests
{
    [Test]
    public void ToDomainString_ItemType()
    {
        Assert.AreEqual("minifig", ItemType.Minifig.ToDomainString());
        Assert.AreEqual("part", ItemType.Part.ToDomainString());
        Assert.AreEqual("book", ItemType.Book.ToDomainString());
        Assert.AreEqual("catalog", ItemType.Catalog.ToDomainString());
        Assert.AreEqual("gear", ItemType.Gear.ToDomainString());
        Assert.AreEqual("instruction", ItemType.Instruction.ToDomainString());
        Assert.AreEqual("original_box", ItemType.OriginalBox.ToDomainString());
        Assert.AreEqual("unsorted_lot", ItemType.UnsortedLot.ToDomainString());
        ItemType? itemType = null;
        Assert.IsNull(itemType?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PartOutItemType()
    {
        Assert.AreEqual("M", PartOutItemType.Minifig.ToDomainString());
        Assert.AreEqual("G", PartOutItemType.Gear.ToDomainString());
        Assert.AreEqual("S", PartOutItemType.Set.ToDomainString());

        PartOutItemType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_OrderStatus()
    {
        Assert.AreEqual("PENDING", OrderStatus.Pending.ToDomainString());
        Assert.AreEqual("UPDATED", OrderStatus.Updated.ToDomainString());
        Assert.AreEqual("PROCESSING", OrderStatus.Processing.ToDomainString());
        Assert.AreEqual("READY", OrderStatus.Ready.ToDomainString());
        Assert.AreEqual("PAID", OrderStatus.Paid.ToDomainString());
        Assert.AreEqual("PACKED", OrderStatus.Packed.ToDomainString());
        Assert.AreEqual("SHIPPED", OrderStatus.Shipped.ToDomainString());
        Assert.AreEqual("RECEIVED", OrderStatus.Received.ToDomainString());
        Assert.AreEqual("COMPLETED", OrderStatus.Completed.ToDomainString());
        Assert.AreEqual("OCR", OrderStatus.Ocr.ToDomainString());
        Assert.AreEqual("NPB", OrderStatus.Npb.ToDomainString());
        Assert.AreEqual("NPX", OrderStatus.Npx.ToDomainString());
        Assert.AreEqual("NRS", OrderStatus.Nrs.ToDomainString());
        Assert.AreEqual("NSS", OrderStatus.Nss.ToDomainString());
        Assert.AreEqual("CANCELLED", OrderStatus.Cancelled.ToDomainString());
        Assert.AreEqual("PURGED", OrderStatus.Purged.ToDomainString());

        OrderStatus? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Condition()
    {
        Assert.AreEqual("N", Condition.New.ToDomainString());
        Assert.AreEqual("U", Condition.Used.ToDomainString());

        Condition? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PriceGuideType()
    {
        Assert.AreEqual("sold", PriceGuideType.Sold.ToDomainString());
        Assert.AreEqual("stock", PriceGuideType.Stock.ToDomainString());

        PriceGuideType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_OrderDirection()
    {
        Assert.AreEqual("in", OrderDirection.In.ToDomainString());
        Assert.AreEqual("out", OrderDirection.Out.ToDomainString());

        OrderDirection? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Direction()
    {
        Assert.AreEqual("in", Direction.In.ToDomainString());
        Assert.AreEqual("out", Direction.Out.ToDomainString());

        Direction? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_InventoryStatusType()
    {
        Assert.AreEqual("Y", InventoryStatusType.Available.ToDomainString());
        Assert.AreEqual("S", InventoryStatusType.InStockRoomA.ToDomainString());
        Assert.AreEqual("B", InventoryStatusType.InStockRoomB.ToDomainString());
        Assert.AreEqual("C", InventoryStatusType.InStockRoomC.ToDomainString());
        Assert.AreEqual("N", InventoryStatusType.Unavailable.ToDomainString());
        Assert.AreEqual("R", InventoryStatusType.Reserved.ToDomainString());

        InventoryStatusType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_CouponStatus()
    {
        Assert.AreEqual("O", CouponStatus.Open.ToDomainString());
        Assert.AreEqual("A", CouponStatus.Redeemed.ToDomainString());
        Assert.AreEqual("D", CouponStatus.Declined.ToDomainString());
        Assert.AreEqual("E", CouponStatus.Expired.ToDomainString());

        CouponStatus? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_ShippingArea()
    {
        Assert.AreEqual("D", ShippingArea.Domestic.ToDomainString());
        Assert.AreEqual("I", ShippingArea.International.ToDomainString());
        Assert.AreEqual("B", ShippingArea.Both.ToDomainString());

        ShippingArea? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_AppearsAs()
    {
        Assert.AreEqual("R", AppearsAs.Regular.ToDomainString());
        Assert.AreEqual("A", AppearsAs.Alternate.ToDomainString());
        Assert.AreEqual("C", AppearsAs.Counterpart.ToDomainString());
        Assert.AreEqual("E", AppearsAs.Extra.ToDomainString());

        AppearsAs? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_RatingTargetRole()
    {
        Assert.AreEqual("S", RatingTargetRole.Seller.ToDomainString());
        Assert.AreEqual("B", RatingTargetRole.Buyer.ToDomainString());

        RatingTargetRole? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_DiscountType()
    {
        Assert.AreEqual("F", DiscountType.Fixed.ToDomainString());
        Assert.AreEqual("S", DiscountType.Percentage.ToDomainString());

        DiscountType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_Completeness()
    {
        Assert.AreEqual("C", Completeness.Complete.ToDomainString());
        Assert.AreEqual("B", Completeness.Incomplete.ToDomainString());
        Assert.AreEqual("S", Completeness.Sealed.ToDomainString());

        Completeness? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_CouponRestrictionType()
    {
        Assert.AreEqual("A", CouponRestrictionType.ApplyToSpecifiedItemType.ToDomainString());
        Assert.AreEqual("E", CouponRestrictionType.ExcludeSpecifiedType.ToDomainString());

        CouponRestrictionType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_EventType()
    {
        Assert.AreEqual("Order", EventType.Order.ToDomainString());
        Assert.AreEqual("Message", EventType.Message.ToDomainString());
        Assert.AreEqual("Feedback", EventType.Feedback.ToDomainString());

        EventType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_RatingType()
    {
        Assert.AreEqual("Complaint", RatingType.Complaint.ToDomainString());
        Assert.AreEqual("Neutral", RatingType.Neutral.ToDomainString());
        Assert.AreEqual("Praise", RatingType.Praise.ToDomainString());

        RatingType? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }

    [Test]
    public void ToDomainString_PaymentStatus()
    {
        Assert.AreEqual("Bounced", PaymentStatus.Bounced.ToDomainString());
        Assert.AreEqual("Clearing", PaymentStatus.Clearing.ToDomainString());
        Assert.AreEqual("Completed", PaymentStatus.Completed.ToDomainString());
        Assert.AreEqual("None", PaymentStatus.None.ToDomainString());
        Assert.AreEqual("Received", PaymentStatus.Received.ToDomainString());
        Assert.AreEqual("Returned", PaymentStatus.Returned.ToDomainString());
        Assert.AreEqual("Sent", PaymentStatus.Sent.ToDomainString());

        PaymentStatus? nullHolder = null;
        Assert.IsNull(nullHolder?.ToDomainString());
    }
}