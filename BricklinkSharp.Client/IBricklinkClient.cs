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
using System.Threading;
using System.Threading.Tasks;

namespace BricklinkSharp.Client;

public interface IBricklinkClient : IDisposable
{
    Task<CatalogItem> GetItemAsync(ItemType type, 
        string no,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<CatalogImage> GetItemImageAsync(ItemType type, 
        string no, 
        int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Uri? GetImage(string number, ItemType type, int? colorId = null, string scheme = "https");
    Uri GetPartImageForColor(string partNo, int colorId, string scheme = "https");

    Uri GetMinifigImage(string number, string scheme = "https");

    Uri GetSetImage(string number, string scheme = "https");

    Uri GetBookImage(string number, string scheme = "https");

    Uri GetGearImage(string number, string scheme = "https");

    Uri GetCatalogImage(string number, string scheme = "https");

    Uri GetInstructionImage(string number, string scheme = "https");

    Uri GetOriginalBoxImage(string number, string scheme = "https");
    Uri? GetThumbnail(string number, ItemType type, int? colorId = null, string scheme = "https");
    Uri GetPartThumbnailForColor(string partNo, int colorId, string scheme = "https");

    Uri GetMinifigThumbnail(string number, string scheme = "https");

    Uri GetSetThumbnail(string number, string scheme = "https");

    Uri GetBookThumbnail(string number, string scheme = "https");

    Uri GetGearThumbnail(string number, string scheme = "https");

    Uri GetCatalogThumbnail(string number, string scheme = "https");

    Uri GetInstructionThumbnail(string number, string scheme = "https");

    Uri GetOriginalBoxThumbnail(string number, string scheme = "https");

    Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https");

    Task<Superset[]> GetSupersetsAsync(ItemType type, 
        string no, 
        int colorId = 0,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no,
        int colorId = 0, PriceGuideType? priceGuideType = null,
        Condition? condition = null, string? countryCode = null,
        string? region = null,
        string? currencyCode = null,
        VatOption? vat = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<KnownColor[]> GetKnownColorsAsync(ItemType type, 
        string no,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Color[]> GetColorListAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Color> GetColorAsync(int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Category[]> GetCategoryListAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Category> GetCategoryAsync(int categoryId, 
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType>? includedItemTypes = null,
        IEnumerable<ItemType>? excludedItemTypes = null,
        IEnumerable<InventoryStatusType>? includedStatusFlags = null,
        IEnumerable<InventoryStatusType>? excludedStatusFlags = null,
        IEnumerable<int>? includedCategoryIds = null,
        IEnumerable<int>? excludedCategoryIds = null,
        IEnumerable<int>? includedColorIds = null,
        IEnumerable<int>? excludedColorIds = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Inventory> GetInventoryAsync(int inventoryId, 
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Inventory> CreateInventoryAsync(NewInventory newInventory, 
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task CreateInventoriesAsync(NewInventory[] newInventories, 
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Inventory> UpdateInventoryAsync(int inventoryId, 
        UpdateInventory updateInventory, 
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task DeleteInventoryAsync(int inventoryId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<ItemMapping[]> GetElementIdAsync(string partNo, 
        int? colorId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<ItemMapping[]> GetItemNumberAsync(string elementId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<ShippingMethod[]> GetShippingMethodListAsync(BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<ShippingMethod> GetShippingMethodAsync(int methodId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Notification[]> GetNotificationsAsync(BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<MemberRating> GetMemberRatingAsync(string username,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Feedback> GetFeedbackAsync(int feedbackId, 
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Feedback> PostFeedbackAsync(int orderId, 
        RatingType rating, 
        string comment,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task ReplyFeedbackAsync(int feedbackId, 
        string reply,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<OrderDetails> GetOrderAsync(int orderId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<OrderMessage[]> GetOrderMessagesAsync(int orderId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Feedback[]> GetOrderFeedbackAsync(int orderId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<OrderDetails> UpdateOrderAsync(int orderId, 
        UpdateOrder updateOrder,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task UpdateOrderStatusAsync(int orderId, 
        OrderStatus status, 
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task UpdatePaymentStatusAsync(int orderId, 
        PaymentStatus status,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task SendDriveThruAsync(int orderId, 
        bool mailCcMe,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
        IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
        IEnumerable<CouponStatus>? excludedCouponStatusTypes = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default);

    Task<Coupon> GetCouponAsync(int couponId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Coupon> CreateCouponAsync(NewCoupon newCoupon,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<Coupon> UpdateCouponAsync(int couponId, 
        UpdateCoupon updateCoupon,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task DeleteCouponAsync(int couponId,
        BricklinkCredentials? credentials = null, 
        CancellationToken cancellationToken = default);

    Task<PartOutResult> GetPartOutValueFromPageAsync(string itemNo, Condition condition = Condition.New,
        bool includeInstructions = true, bool breakMinifigs = false,
        bool includeBox = false, bool includeExtraParts = false, bool breakSetsInSet = false,
        PartOutItemType itemType = PartOutItemType.Set, string? currencyCode = null,
        CancellationToken cancellationToken = default);
}