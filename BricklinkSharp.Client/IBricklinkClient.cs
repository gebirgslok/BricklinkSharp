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

using BricklinkSharp.Client.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BricklinkSharp.Client;

public interface IBricklinkClient : IDisposable
{
    Task<CatalogItem> GetItemAsync(ItemType type, string no, CancellationToken cancellationToken = default);

    Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId, CancellationToken cancellationToken = default);

    Uri GetPartImageForColor(string partNo, int colorId, string scheme = "https");

    Uri GetMinifigImage(string number, string scheme = "https");

    Uri GetSetImage(string number, string scheme = "https");

    Uri GetBookImage(string number, string scheme = "https");

    Uri GetGearImage(string number, string scheme = "https");

    Uri GetCatalogImage(string number, string scheme = "https");

    Uri GetInstructionImage(string number, string scheme = "https");

    Uri GetOriginalBoxImage(string number, string scheme = "https");

    Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https");

    Task<Superset[]> GetSupersetsAsync(ItemType type, string no, int colorId = 0, CancellationToken cancellationToken = default);

    Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null,
        CancellationToken cancellationToken = default);

    Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no,
        int colorId = 0, PriceGuideType? priceGuideType = null,
        Condition? condition = null, string? countryCode = null,
        string? region = null,
        string? currencyCode = null,
        CancellationToken cancellationToken = default);

    Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no, CancellationToken cancellationToken = default);

    Task<Color[]> GetColorListAsync(CancellationToken cancellationToken = default);

    Task<Color> GetColorAsync(int colorId, CancellationToken cancellationToken = default);

    Task<Category[]> GetCategoryListAsync(CancellationToken cancellationToken = default);

    Task<Category> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType>? includedItemTypes = null,
        IEnumerable<ItemType>? excludedItemTypes = null,
        IEnumerable<InventoryStatusType>? includedStatusFlags = null,
        IEnumerable<InventoryStatusType>? excludedStatusFlags = null,
        IEnumerable<int>? includedCategoryIds = null,
        IEnumerable<int>? excludedCategoryIds = null,
        IEnumerable<int>? includedColorIds = null,
        IEnumerable<int>? excludedColorIds = null,
        CancellationToken cancellationToken = default);

    Task<Inventory> GetInventoryAsync(int inventoryId, CancellationToken cancellationToken = default);

    Task<Inventory> CreateInventoryAsync(NewInventory newInventory, CancellationToken cancellationToken = default);

    Task CreateInventoriesAsync(NewInventory[] newInventories, CancellationToken cancellationToken = default);

    Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdateInventory updateInventory, 
        CancellationToken cancellationToken = default);

    Task DeleteInventoryAsync(int inventoryId, CancellationToken cancellationToken = default);

    Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId, CancellationToken cancellationToken = default);

    Task<ItemMapping[]> GetItemNumberAsync(string elementId, CancellationToken cancellationToken = default);

    Task<ShippingMethod[]> GetShippingMethodListAsync(CancellationToken cancellationToken = default);

    Task<ShippingMethod> GetShippingMethodAsync(int methodId, CancellationToken cancellationToken = default);

    Task<Notification[]> GetNotificationsAsync(CancellationToken cancellationToken = default);

    Task<MemberRating> GetMemberRatingAsync(string username, CancellationToken cancellationToken = default);

    Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null, CancellationToken cancellationToken = default);

    Task<Feedback> GetFeedbackAsync(int feedbackId, CancellationToken cancellationToken = default);

    Task<Feedback> PostFeedbackAsync(int orderId, RatingType rating, string comment, CancellationToken cancellationToken = default);

    Task ReplyFeedbackAsync(int feedbackId, string reply, CancellationToken cancellationToken = default);

    Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        CancellationToken cancellationToken = default);

    Task<OrderDetails> GetOrderAsync(int orderId, CancellationToken cancellationToken = default);

    Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default);

    Task<OrderMessage[]> GetOrderMessagesAsync(int orderId, CancellationToken cancellationToken = default);

    Task<Feedback[]> GetOrderFeedbackAsync(int orderId, CancellationToken cancellationToken = default);

    Task<OrderDetails> UpdateOrderAsync(int orderId, UpdateOrder updateOrder, CancellationToken cancellationToken = default);

    Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);

    Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status, CancellationToken cancellationToken = default);

    Task SendDriveThruAsync(int orderId, bool mailCcMe, CancellationToken cancellationToken = default);

    Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
        IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
        IEnumerable<CouponStatus>? excludedCouponStatusTypes = null,
        CancellationToken cancellationToken = default);

    Task<Coupon> GetCouponAsync(int couponId, CancellationToken cancellationToken = default);

    Task<Coupon> CreateCouponAsync(NewCoupon newCoupon, CancellationToken cancellationToken = default);

    Task<Coupon> UpdateCouponAsync(int couponId, UpdateCoupon updateCoupon, CancellationToken cancellationToken = default);

    Task DeleteCouponAsync(int couponId, CancellationToken cancellationToken = default);

    Task<PartOutResult> GetPartOutValueFromPageAsync(string itemNo, Condition condition = Condition.New,
        bool includeInstructions = true, bool breakMinifigs = false,
        bool includeBox = false, bool includeExtraParts = false, bool breakSetsInSet = false,
        PartOutItemType itemType = PartOutItemType.Set, string? currencyCode = null,
        CancellationToken cancellationToken = default);
}