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

    private readonly BricklinkRequestExecutor _executor;
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    private bool _isDisposed;

    public BricklinkClient(BricklinkRequestExecutor executor, HttpClient httpClient, bool disposeHttpClient)
    {
        _executor = executor;
        _httpClient = httpClient;
        _disposeHttpClient = disposeHttpClient;
    }

    ~BricklinkClient()
    {
        Dispose(false);
    }

    private static string? BuildIncludeExcludeParameter<T>(IEnumerable<T>? includes, IEnumerable<T>? excludes,
        Func<T, string> toStringFunc)
    {
        var allParameters = new List<string>();

        if (includes != null)
        {
            allParameters.AddRange(includes.Select(toStringFunc.Invoke));
        }

        if (excludes != null)
        {
            allParameters.AddRange(excludes.Select(excl => $"-{toStringFunc.Invoke(excl)}"));
        }

        if (allParameters.Any())
        {
            return allParameters.ToConcatenatedString();
        }

        return null;
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

    public Task<CatalogItem> GetItemAsync(ItemType type,
        string no,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<CatalogItem>(
            new BricklinkRequest(BricklinkApiResourceType.Item, HttpVerb.Get, $"items/{type.ToDomainString()}/{no}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<CatalogImage> GetItemImageAsync(ItemType type,
        string no,
        int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<CatalogImage>(
            new BricklinkRequest(BricklinkApiResourceType.ItemImage, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{no}/images/{colorId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Uri? GetImage(string number, ItemType type, int? colorId = null, string scheme = "https")
    {
        return type switch
        {
            ItemType.Minifig => GetMinifigImage(number, scheme),
            ItemType.Part => colorId.HasValue ? GetPartImageForColor(number, colorId.Value, scheme) : null,
            ItemType.Set => GetSetImage(number, scheme),
            ItemType.Book => GetBookImage(number, scheme),
            ItemType.Gear => GetGearImage(number, scheme),
            ItemType.Catalog => GetCatalogImage(number, scheme),
            ItemType.Instruction => GetInstructionImage(number, scheme),
            ItemType.OriginalBox => GetOriginalBoxImage(number, scheme),
            _ => null
        };
    }

    public Uri GetPartImageForColor(string partNo, int colorId, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_partImageUrlTemplate, colorId.ToString(), partNo)}");
    }

    public Uri GetMinifigImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_minifigImageUrlTemplate, number)}");
    }

    public Uri GetSetImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_setImageUrlTemplate, number)}");
    }

    public Uri GetBookImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_bookImageUrlTemplate, number)}");
    }

    public Uri GetGearImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_gearImageUrlTemplate, number)}");
    }

    public Uri GetCatalogImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_catalogImageUrlTemplate, number)}");
    }

    public Uri GetInstructionImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_instructionImageUrlTemplate, number)}");
    }

    public Uri GetOriginalBoxImage(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_originalBoxImageUrlTemplate, number)}");
    }

    public Uri? GetThumbnail(string number, ItemType type, int? colorId = null, string scheme = "https")
    {
        return type switch
        {
            ItemType.Minifig => GetMinifigThumbnail(number, scheme),
            ItemType.Part => colorId.HasValue ? GetPartThumbnailForColor(number, colorId.Value, scheme) : null,
            ItemType.Set => GetSetThumbnail(number, scheme),
            ItemType.Book => GetBookThumbnail(number, scheme),
            ItemType.Gear => GetGearThumbnail(number, scheme),
            ItemType.Catalog => GetCatalogThumbnail(number, scheme),
            ItemType.Instruction => GetInstructionThumbnail(number, scheme),
            ItemType.OriginalBox => GetOriginalBoxThumbnail(number, scheme),
            _ => null
        };
    }

    public Uri GetPartThumbnailForColor(string partNo, int colorId, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_partThumbnailUrlTemplate, colorId.ToString(), partNo)}");
    }

    public Uri GetMinifigThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_minifigThumbnailUrlTemplate, number)}");
    }

    public Uri GetSetThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_setThumbnailUrlTemplate, number)}");
    }

    public Uri GetBookThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_bookThumbnailUrlTemplate, number)}");
    }

    public Uri GetGearThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_gearThumbnailUrlTemplate, number)}");
    }

    public Uri GetCatalogThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_catalogThumbnaileUrlTemplate, number)}");
    }

    public Uri GetInstructionThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_instructionThumbnailUrlTemplate, number)}");
    }

    public Uri GetOriginalBoxThumbnail(string number, string scheme = "https")
    {
        return new Uri($"{scheme}:{string.Format(_originalBoxThumbnailUrlTemplate, number)}");
    }

    public Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https")
    {
        return new Uri($"{scheme}:{imageUrl}");
    }

    public Task<Superset[]> GetSupersetsAsync(ItemType type, string no,
        int colorId = 0,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Superset[]>(
            new BricklinkRequest(BricklinkApiResourceType.Superset, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{no}/supersets")
                .WithQueryParameter("color_id", colorId > 0 ? colorId : null)
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Subset[]> GetSubsetsAsync(ItemType type, string no,
        int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null,
        bool? breakSubsets = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Subset[]>(
            new BricklinkRequest(BricklinkApiResourceType.Subset, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{no}/subsets")
                .WithQueryParameter("color_id", colorId)
                .WithQueryParameter("box", includeOriginalBox)
                .WithQueryParameter("instruction", includeInstruction)
                .WithQueryParameter("break_minifigs", breakMinifigs)
                .WithQueryParameter("break_subsets", breakSubsets)
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, int colorId = 0,
        PriceGuideType? priceGuideType = null, Condition? condition = null,
        string? countryCode = null, string? region = null, string? currencyCode = null,
        VatOption? vat = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<PriceGuide>(
            new BricklinkRequest(BricklinkApiResourceType.PriceGuide, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{no}/price")
                .WithQueryParameter("color_id", colorId)
                .WithQueryParameter("guide_type", priceGuideType?.ToDomainString())
                .WithQueryParameter("new_or_used", condition?.ToDomainString())
                .WithQueryParameter("country_code", countryCode)
                .WithQueryParameter("region", region)
                .WithQueryParameter("currency_code", currencyCode)
                .WithQueryParameter("vat", vat?.ToDomainString())
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<KnownColor[]>(
            new BricklinkRequest(BricklinkApiResourceType.KnownColor, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{no}/colors")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Color[]> GetColorListAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Color[]>(
            new BricklinkRequest(BricklinkApiResourceType.ColorList, HttpVerb.Get, "colors")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Color> GetColorAsync(int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Color>(
            new BricklinkRequest(BricklinkApiResourceType.Color, HttpVerb.Get, $"colors/{colorId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Category[]> GetCategoryListAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Category[]>(
            new BricklinkRequest(BricklinkApiResourceType.CategoryList, HttpVerb.Get, "categories")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Category> GetCategoryAsync(int categoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Category>(
            new BricklinkRequest(BricklinkApiResourceType.Category, HttpVerb.Get, $"categories/{categoryId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType>? includedItemTypes = null,
        IEnumerable<ItemType>? excludedItemTypes = null,
        IEnumerable<InventoryStatusType>? includedStatusFlags = null,
        IEnumerable<InventoryStatusType>? excludedStatusFlags = null,
        IEnumerable<int>? includedCategoryIds = null,
        IEnumerable<int>? excludedCategoryIds = null,
        IEnumerable<int>? includedColorIds = null,
        IEnumerable<int>? excludedColorIds = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Inventory[]>(
            new BricklinkRequest(BricklinkApiResourceType.InventoryList, HttpVerb.Get, "inventories")
                .WithQueryParameter("item_type", BuildIncludeExcludeParameter(includedItemTypes,
                    excludedItemTypes, t => t.ToDomainString()))
                .WithQueryParameter("status", BuildIncludeExcludeParameter(includedStatusFlags,
                    excludedStatusFlags, f => f.ToDomainString()))
                .WithQueryParameter("category_id", BuildIncludeExcludeParameter(includedCategoryIds,
                    excludedCategoryIds, categoryId => categoryId.ToString()))
                .WithQueryParameter("color_id", BuildIncludeExcludeParameter(includedColorIds, excludedColorIds,
                    colorId => colorId.ToString()))
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Inventory> GetInventoryAsync(int inventoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Inventory>(
            new BricklinkRequest(BricklinkApiResourceType.Inventory, HttpVerb.Get, $"inventories/{inventoryId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        newInventory.ValidateThrowException();

        return await _executor.SendAsync<Inventory>(
            new BricklinkRequest(BricklinkApiResourceType.Inventory, HttpVerb.Post, "inventories")
                .WithBody(newInventory)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateInventoriesAsync(NewInventory[] newInventories,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var newInventory in newInventories)
        {
            newInventory.ValidateThrowException();
        }

        await _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.Inventory, HttpVerb.Post, "inventories")
                .WithBody(newInventories)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Inventory> UpdateInventoryAsync(int inventoryId,
        UpdateInventory updateInventory,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        updateInventory.ValidateThrowException();

        return await _executor.SendAsync<Inventory>(
            new BricklinkRequest(BricklinkApiResourceType.Inventory, HttpVerb.Put, $"inventories/{inventoryId}")
                .WithBody(updateInventory)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public Task DeleteInventoryAsync(int inventoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.Inventory, HttpVerb.Delete, $"inventories/{inventoryId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ItemMapping[]>(
            new BricklinkRequest(BricklinkApiResourceType.ElementId, HttpVerb.Get, $"item_mapping/part/{partNo}")
                .WithQueryParameter("color_id", colorId)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ItemMapping[]> GetItemNumberAsync(string elementId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ItemMapping[]>(
            new BricklinkRequest(BricklinkApiResourceType.ItemNumber, HttpVerb.Get, $"item_mapping/{elementId}")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ShippingMethod[]> GetShippingMethodListAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ShippingMethod[]>(
            new BricklinkRequest(BricklinkApiResourceType.ShippingMethod, HttpVerb.Get, "settings/shipping_methods")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ShippingMethod> GetShippingMethodAsync(int methodId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ShippingMethod>(
            new BricklinkRequest(BricklinkApiResourceType.ShippingMethod, HttpVerb.Get,
                    $"settings/shipping_methods/{methodId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Notification[]> GetNotificationsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Notification[]>(
            new BricklinkRequest(BricklinkApiResourceType.Notification, HttpVerb.Get, "notifications")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<MemberRating> GetMemberRatingAsync(string username,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<MemberRating>(
            new BricklinkRequest(BricklinkApiResourceType.MemberRating, HttpVerb.Get, $"members/{username}/ratings")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback[]>(
            new BricklinkRequest(BricklinkApiResourceType.FeedbackList, HttpVerb.Get, "feedback")
                .WithQueryParameter("direction", direction?.ToString().ToLowerInvariant())
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback> GetFeedbackAsync(int feedbackId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback>(
            new BricklinkRequest(BricklinkApiResourceType.Feedback, HttpVerb.Get, $"feedback/{feedbackId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback> PostFeedbackAsync(int orderId,
        RatingType rating,
        string comment,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        var body = new FeedbackBase
        {
            Comment = comment,
            OrderId = orderId,
            Rating = rating
        };

        return _executor.SendAsync<Feedback>(
            new BricklinkRequest(BricklinkApiResourceType.Feedback, HttpVerb.Post, "feedback")
                .WithBody(body)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task ReplyFeedbackAsync(int feedbackId,
        string reply,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.Feedback, HttpVerb.Post, $"feedback/{feedbackId}/reply")
                .WithBody(new { reply })
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Order[]>(
            new BricklinkRequest(BricklinkApiResourceType.Order, HttpVerb.Get, "orders")
                .WithQueryParameter("direction", direction.ToDomainString())
                .WithQueryParameter("status", BuildIncludeExcludeParameter(includedStatusFlags,
                    excludedStatusFlags, f => f.ToDomainString()))
                .WithQueryParameter("filed", filed)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<OrderDetails> GetOrderAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<OrderDetails>(
            new BricklinkRequest(BricklinkApiResourceType.Order, HttpVerb.Get, $"orders/{orderId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        var itemsBatches = await _executor.SendAsync<OrderItem[][]>(
                new BricklinkRequest(BricklinkApiResourceType.OrderItem, HttpVerb.Get, $"orders/{orderId}/items")
                    .WithCredentials(credentials),
                cancellationToken)
            .ConfigureAwait(false);

        return itemsBatches.ToList();
    }

    public Task<OrderMessage[]> GetOrderMessagesAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<OrderMessage[]>(
            new BricklinkRequest(BricklinkApiResourceType.OrderMessage, HttpVerb.Get, $"orders/{orderId}/messages")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback[]> GetOrderFeedbackAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback[]>(
            new BricklinkRequest(BricklinkApiResourceType.OrderFeedback, HttpVerb.Get, $"orders/{orderId}/feedback")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task UpdateOrderStatusAsync(int orderId,
        OrderStatus status,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.OrderStatus, HttpVerb.Put, $"orders/{orderId}/status")
                .WithBody(new { field = "status", value = status.ToDomainString() })
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task UpdatePaymentStatusAsync(int orderId,
        PaymentStatus status,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.PaymentStatus, HttpVerb.Put,
                    $"orders/{orderId}/payment_status")
                .WithBody(new { field = "payment_status", value = status.ToDomainString() })
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task SendDriveThruAsync(int orderId,
        bool mailCcMe,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.DriveThru, HttpVerb.Post, $"orders/{orderId}/drive_thru")
                .WithQueryParameter("mail_me", mailCcMe)
                .WithExpectedMetaCode(204)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<OrderDetails> UpdateOrderAsync(int orderId,
        UpdateOrder updateOrder,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<OrderDetails>(
            new BricklinkRequest(BricklinkApiResourceType.Order, HttpVerb.Put, $"orders/{orderId}")
                .WithBody(updateOrder)
                .WithCredentials(credentials),
            cancellationToken);
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
        return _executor.SendAsync<Coupon[]>(
            new BricklinkRequest(BricklinkApiResourceType.Coupon, HttpVerb.Get, "coupons")
                .WithQueryParameter("direction", direction.ToDomainString())
                .WithQueryParameter("status", BuildIncludeExcludeParameter(includedCouponStatusTypes,
                    excludedCouponStatusTypes, f => f.ToDomainString()))
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Coupon> GetCouponAsync(int couponId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Coupon>(
            new BricklinkRequest(BricklinkApiResourceType.Coupon, HttpVerb.Get, $"coupons/{couponId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task DeleteCouponAsync(int couponId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(BricklinkApiResourceType.Coupon, HttpVerb.Delete, $"coupons/{couponId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<Coupon> CreateCouponAsync(NewCoupon newCoupon,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        newCoupon.ValidateThrowException();

        return await _executor.SendAsync<Coupon>(
            new BricklinkRequest(BricklinkApiResourceType.Coupon, HttpVerb.Post, "coupons")
                .WithBody(newCoupon)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Coupon> UpdateCouponAsync(int couponId,
        UpdateCoupon updateCoupon,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        updateCoupon.ValidateThrowException();

        return await _executor.SendAsync<Coupon>(
            new BricklinkRequest(BricklinkApiResourceType.Coupon, HttpVerb.Put, $"coupons/{couponId}")
                .WithBody(updateCoupon)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }
}
