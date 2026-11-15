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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client;

internal sealed class BricklinkClient : IBricklinkClient
{
    private const string _partImageUrlTemplate = "//img.bricklink.com/ItemImage/PN/{0}/{1}.png";
    private const string _minifigImageUrlTemplate = "//img.bricklink.com/ItemImage/MN/0/{0}.png";
    private const string _setImageUrlTemplate = "//img.bricklink.com/ItemImage/SN/0/{0}.png";
    private const string _bookImageUrlTemplate = "//img.bricklink.com/ItemImage/BN/0/{0}.png";
    private const string _gearImageUrlTemplate = "//img.bricklink.com/ItemImage/GN/0/{0}.png";
    private const string _catalogImageUrlTemplate = "//img.bricklink.com/ItemImage/CN/0/{0}.png";
    private const string _instructionImageUrlTemplate = "//img.bricklink.com/ItemImage/IN/0/{0}.png";
    private const string _originalBoxImageUrlTemplate = "//img.bricklink.com/ItemImage/ON/0/{0}.png";

    private const string _partThumbnailUrlTemplate = "//img.bricklink.com/P/{0}/{1}.jpg";
    private const string _minifigThumbnailUrlTemplate = "//img.bricklink.com/M/{0}.jpg";
    private const string _setThumbnailUrlTemplate = "//img.bricklink.com/S/{0}.jpg";
    private const string _bookThumbnailUrlTemplate = "//img.bricklink.com/B/{0}.jpg";
    private const string _gearThumbnailUrlTemplate = "//img.bricklink.com/G/{0}.jpg";
    private const string _catalogThumbnaileUrlTemplate = "//img.bricklink.com/C/{0}.jpg";
    private const string _instructionThumbnailUrlTemplate = "//img.bricklink.com/I/{0}.jpg";
    private const string _originalBoxThumbnailUrlTemplate = "//img.bricklink.com/ON/{0}.jpg";

    private readonly IEndpointSender _sender;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    private bool _isDisposed;

    public BricklinkClient(BricklinkRequestExecutor executor, HttpClient httpClient, bool disposeHttpClient)
    {
        _sender = executor;
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
    }

    ~BricklinkClient()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            if (_disposeHttpClient)
            {
                _httpClient.Dispose();
            }
        }

        _isDisposed = true;
    }

    public Task<CatalogItem> GetCatalogItemAsync(ItemType type,
        string itemNumber,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.CatalogItem(type, itemNumber), credentials, cancellationToken);
    }

    public Task<CatalogImage> GetCatalogImageAsync(ItemType type,
        string itemNumber,
        int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.CatalogImage(type, itemNumber, colorId), credentials, cancellationToken);
    }

    public Uri? GetImage(string itemNumber, ItemType type, int? colorId = null, string scheme = "https")
    {
        return type switch
        {
            ItemType.Minifig => GetMinifigImage(itemNumber, scheme),
            ItemType.Part => colorId.HasValue ? GetPartImageForColor(itemNumber, colorId.Value, scheme) : null,
            ItemType.Set => GetSetImage(itemNumber, scheme),
            ItemType.Book => GetBookImage(itemNumber, scheme),
            ItemType.Gear => GetGearImage(itemNumber, scheme),
            ItemType.Catalog => GetCatalogImage(itemNumber, scheme),
            ItemType.Instruction => GetInstructionImage(itemNumber, scheme),
            ItemType.OriginalBox => GetOriginalBoxImage(itemNumber, scheme),
            _ => null
        };
    }

    public Uri GetPartImageForColor(string itemNumber, int colorId, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_partImageUrlTemplate, colorId.ToString(), itemNumber)}");
    }

    public Uri GetMinifigImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_minifigImageUrlTemplate, itemNumber)}");
    }

    public Uri GetSetImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_setImageUrlTemplate, itemNumber)}");
    }

    public Uri GetBookImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_bookImageUrlTemplate, itemNumber)}");
    }

    public Uri GetGearImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_gearImageUrlTemplate, itemNumber)}");
    }

    public Uri GetCatalogImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_catalogImageUrlTemplate, itemNumber)}");
    }

    public Uri GetInstructionImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_instructionImageUrlTemplate, itemNumber)}");
    }

    public Uri GetOriginalBoxImage(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_originalBoxImageUrlTemplate, itemNumber)}");
    }

    public Uri? GetThumbnail(string itemNumber, ItemType type, int? colorId = null, string scheme = "https")
    {
        return type switch
        {
            ItemType.Minifig => GetMinifigThumbnail(itemNumber, scheme),
            ItemType.Part => colorId.HasValue ? GetPartThumbnailForColor(itemNumber, colorId.Value, scheme) : null,
            ItemType.Set => GetSetThumbnail(itemNumber, scheme),
            ItemType.Book => GetBookThumbnail(itemNumber, scheme),
            ItemType.Gear => GetGearThumbnail(itemNumber, scheme),
            ItemType.Catalog => GetCatalogThumbnail(itemNumber, scheme),
            ItemType.Instruction => GetInstructionThumbnail(itemNumber, scheme),
            ItemType.OriginalBox => GetOriginalBoxThumbnail(itemNumber, scheme),
            _ => null
        };
    }

    public Uri GetPartThumbnailForColor(string itemNumber, int colorId, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_partThumbnailUrlTemplate, colorId.ToString(), itemNumber)}");
    }

    public Uri GetMinifigThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_minifigThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri GetSetThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_setThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri GetBookThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_bookThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri GetGearThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_gearThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri GetCatalogThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_catalogThumbnaileUrlTemplate, itemNumber)}");
    }

    public Uri GetInstructionThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_instructionThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri GetOriginalBoxThumbnail(string itemNumber, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_originalBoxThumbnailUrlTemplate, itemNumber)}");
    }

    public Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https")
    {
        return new Uri($"{scheme}:{imageUrl}");
    }

    public Task<Superset[]> GetSupersetsAsync(ItemType type, string itemNumber,
        int colorId = 0,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Supersets(type, itemNumber, colorId), credentials, cancellationToken);
    }

    public Task<Subset[]> GetSubsetsAsync(ItemType type, string itemNumber,
        int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null,
        bool? breakSubsets = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Subsets(type, itemNumber, colorId, includeOriginalBox, includeInstruction, breakMinifigs,
                breakSubsets), credentials, cancellationToken);
    }

    public Task<PriceGuide> GetPriceGuideAsync(ItemType type, string itemNumber, int colorId = 0,
        PriceGuideSource? source = null, Condition? condition = null,
        string? countryCode = null, string? region = null, string? currencyCode = null,
        VatOption? vat = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.PriceGuide(type, itemNumber, colorId, source, condition, countryCode, region,
                currencyCode, vat), credentials, cancellationToken);
    }

    public Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string itemNumber,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.KnownColors(type, itemNumber), credentials, cancellationToken);
    }

    public Task<Color[]> GetColorsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Colors(), credentials, cancellationToken);
    }

    public Task<Color> GetColorAsync(int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Color(colorId), credentials, cancellationToken);
    }

    public Task<Category[]> GetCategoriesAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Categories(), credentials, cancellationToken);
    }

    public Task<Category> GetCategoryAsync(int categoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Category(categoryId), credentials, cancellationToken);
    }

    public Task<Inventory[]> GetInventoriesAsync(IEnumerable<ItemType>? includedItemTypes = null,
        IEnumerable<ItemType>? excludedItemTypes = null,
        IEnumerable<InventoryStatus>? includedStatusFlags = null,
        IEnumerable<InventoryStatus>? excludedStatusFlags = null,
        IEnumerable<int>? includedCategoryIds = null,
        IEnumerable<int>? excludedCategoryIds = null,
        IEnumerable<int>? includedColorIds = null,
        IEnumerable<int>? excludedColorIds = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Inventories(includedItemTypes, excludedItemTypes, includedStatusFlags,
                excludedStatusFlags, includedCategoryIds, excludedCategoryIds, includedColorIds, excludedColorIds), credentials, cancellationToken);
    }

    public Task<Inventory> GetInventoryAsync(int inventoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Inventory(inventoryId), credentials, cancellationToken);
    }

    public Task<Inventory> CreateInventoryAsync(NewInventory newInventory,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.CreateInventory(newInventory), credentials, cancellationToken);
    }

    public Task CreateInventoriesAsync(NewInventory[] newInventories,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.CreateInventories(newInventories), credentials, cancellationToken);
    }

    public Task<Inventory> UpdateInventoryAsync(int inventoryId,
        UpdateInventory updateInventory,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.UpdateInventory(inventoryId, updateInventory), credentials, cancellationToken);
    }

    public Task DeleteInventoryAsync(int inventoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.DeleteInventory(inventoryId), credentials, cancellationToken);
    }

    public Task<ItemMapping[]> GetItemMappingsByItemAsync(string itemNumber, int? colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.ItemMappingsByItemNumber(itemNumber, colorId), credentials, cancellationToken);
    }

    public Task<ItemMapping[]> GetItemMappingsByElementIdAsync(string elementId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.ItemMappingsByElementId(elementId), credentials, cancellationToken);
    }

    public Task<ShippingMethod[]> GetShippingMethodsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.ShippingMethods(), credentials, cancellationToken);
    }

    public Task<ShippingMethod> GetShippingMethodAsync(int shippingMethodId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.ShippingMethod(shippingMethodId), credentials, cancellationToken);
    }

    public Task<PushNotification[]> GetPushNotificationsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.PushNotifications(), credentials, cancellationToken);
    }

    public Task<MemberRating> GetMemberRatingAsync(string username,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.MemberRating(username), credentials, cancellationToken);
    }

    public Task<Feedback[]> GetFeedbacksAsync(Direction? direction = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Feedbacks(direction), credentials, cancellationToken);
    }

    public Task<Feedback> GetFeedbackAsync(int feedbackId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Feedback(feedbackId), credentials, cancellationToken);
    }

    public Task<Feedback> PostFeedbackAsync(int orderId,
        Rating rating,
        string comment,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.PostFeedback(orderId, rating, comment), credentials, cancellationToken);
    }

    public Task ReplyToFeedbackAsync(int feedbackId,
        string reply,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.ReplyToFeedback(feedbackId, reply), credentials, cancellationToken);
    }

    public Task<Order[]> GetOrdersAsync(Direction direction = Direction.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Orders(direction, includedStatusFlags, excludedStatusFlags, filed), credentials, cancellationToken);
    }

    public Task<OrderDetails> GetOrderAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Order(orderId), credentials, cancellationToken);
    }

    public Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.OrderItems(orderId), credentials, cancellationToken);
    }

    public Task<OrderMessage[]> GetOrderMessagesAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.OrderMessages(orderId), credentials, cancellationToken);
    }

    public Task<Feedback[]> GetOrderFeedbacksAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.OrderFeedbacks(orderId), credentials, cancellationToken);
    }

    public Task UpdateOrderStatusAsync(int orderId,
        OrderStatus status,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.UpdateOrderStatus(orderId, status), credentials, cancellationToken);
    }

    public Task UpdatePaymentStatusAsync(int orderId,
        PaymentStatus status,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.UpdatePaymentStatus(orderId, status), credentials, cancellationToken);
    }

    public Task SendDriveThruAsync(int orderId,
        bool mailCcMe,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.SendDriveThru(orderId, mailCcMe), credentials, cancellationToken);
    }

    public Task<OrderDetails> UpdateOrderAsync(int orderId,
        UpdateOrder updateOrder,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.UpdateOrder(orderId, updateOrder), credentials, cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
        IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
        IEnumerable<CouponStatus>? excludedCouponStatusTypes = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Coupons(direction, includedCouponStatusTypes, excludedCouponStatusTypes), credentials, cancellationToken);
    }

    public Task<Coupon> GetCouponAsync(int couponId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.Coupon(couponId), credentials, cancellationToken);
    }

    public Task DeleteCouponAsync(int couponId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.DeleteCoupon(couponId), credentials, cancellationToken);
    }

    public Task<Coupon> CreateCouponAsync(NewCoupon newCoupon,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.CreateCoupon(newCoupon), credentials, cancellationToken);
    }

    public Task<Coupon> UpdateCouponAsync(int couponId,
        UpdateCoupon updateCoupon,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _sender.SendAsync(Endpoints.UpdateCoupon(couponId, updateCoupon), credentials, cancellationToken);
    }
}
