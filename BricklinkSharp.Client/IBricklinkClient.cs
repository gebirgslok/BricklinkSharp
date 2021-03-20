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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BricklinkSharp.Client
{
    public interface IBricklinkClient : IDisposable
    {
        Task<CatalogItem> GetItemAsync(ItemType type, string no);

        Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId);

        Uri GetPartImageForColor(string partNo, int colorId, string scheme = "https");

        Uri GetMinifigImage(string number, string scheme = "https");

        Uri GetSetImage(string number, string scheme = "https");

        Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https");

        Task<Superset[]> GetSupersetsAsync(ItemType type, string no, int colorId);

        Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
            bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null);

        Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, 
            int colorId = 0, PriceGuideType? priceGuideType = null,
            Condition? condition = null, string? countryCode = null, 
            string? region = null, 
            string? currencyCode = null);

        Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no);

        Task<Color[]> GetColorListAsync();

        Task<Color> GetColorAsync(int colorId);

        Task<Category[]> GetCategoryListAsync();

        Task<Category> GetCategoryAsync(int categoryId);

        Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType>? includedItemTypes = null, 
            IEnumerable<ItemType>? excludedItemTypes = null,
            IEnumerable<InventoryStatusType>? includedStatusFlags = null,
            IEnumerable<InventoryStatusType>? excludedStatusFlags = null,
            IEnumerable<int>? includedCategoryIds = null, 
            IEnumerable<int>? excludedCategoryIds = null,
            IEnumerable<int>? includedColorIds = null,
            IEnumerable<int>? excludedColorIds = null);

        Task<Inventory> GetInventoryAsync(int inventoryId);

        Task<Inventory> CreateInventoryAsync(NewInventory newInventory);

        Task CreateInventoriesAsync(NewInventory[] newInventories);

        Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdateInventory updatedInventory);

        Task DeleteInventoryAsync(int inventoryId);

        Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId);

        Task<ItemMapping[]> GetItemNumberAsync(string elementId);

        Task<ShippingMethod[]> GetShippingMethodListAsync();

        Task<ShippingMethod> GetShippingMethodAsync(int methodId);

        Task<Notification[]> GetNotificationsAsync();

        Task<MemberRating> GetMemberRatingAsync(string username);

        Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null);

        Task<Feedback> GetFeedbackAsync(int feedbackId);

        Task<Feedback> PostFeedbackAsync(int orderId, RatingType rating, string comment);

        Task ReplyFeedbackAsync(int feedbackId, string reply);

        Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
            IEnumerable<OrderStatus>? includedStatusFlags = null,
            IEnumerable<OrderStatus>? excludedStatusFlags = null, 
            bool filed = false);

        Task<OrderDetails> GetOrderAsync(int orderId);

        Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId);

        Task<OrderMessage[]> GetOrderMessagesAsync(int orderId);

        Task<Feedback[]> GetOrderFeedbackAsync(int orderId);

        Task<OrderDetails> UpdateOrderAsync(int orderId, UpdateOrder updateOrder);

        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

        Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status);

        Task SendDriveThruAsync(int orderId, bool mailCcMe);

        Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
            IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
            IEnumerable<CouponStatus>? excludedCouponStatusTypes = null);

        Task<Coupon> GetCouponAsync(int couponId);

        Task<Coupon> CreateCouponAsync(NewCoupon newCoupon);

        Task<Coupon> UpdateCouponAsync(int couponId, UpdateCoupon updateCoupon);

        Task DeleteCouponAsync(int couponId);

        Task<PartOutResult> GetPartOutValueAsync(string itemNo, Condition condition = Condition.New, 
            bool includeInstructions = true, bool breakMinifigs = false, 
            bool includeBox = false, bool includeExtraParts = false, bool breakSetsInSet = false, 
            PartOutItemType itemType = PartOutItemType.Set);
    }
}
