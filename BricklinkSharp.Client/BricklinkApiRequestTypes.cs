using System;
using System.Collections.Generic;
using System.Text;

namespace BricklinkSharp.Client
{
    public enum BricklinkApiRequestTypes
    {
        Unknown,
        Item,
        ItemImage,
        ItemNumber,
        Order,
        OrderItem,
        OrderDetails,
        OrderMessage,
        OrderFeedback,
        OrderStatus,
        PaymentStatus,
        DriveThru,
        Superset,
        Subset,
        PriceGuide,        
        Color,
        KnownColor,
        ColorList,
        Category,
        CategoryList,        
        Inventory,
        InventoryList,
        ElementId,
        ShippingMethod,
        Notification,
        MemberRating,
        Feedback,
        FeedbackList,
        Coupon,
        PartoutValue
    }
}
