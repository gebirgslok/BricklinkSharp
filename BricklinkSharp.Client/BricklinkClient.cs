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

    public Task<CatalogItem> GetCatalogItemAsync(ItemType type,
        string itemNumber,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<CatalogItem>(
            new BricklinkRequest(ApiResourceType.Item, HttpVerb.Get, $"items/{type.ToDomainString()}/{itemNumber}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<CatalogImage> GetCatalogImageAsync(ItemType type,
        string itemNumber,
        int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<CatalogImage>(
            new BricklinkRequest(ApiResourceType.ItemImage, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{itemNumber}/images/{colorId}")
                .WithCredentials(credentials),
            cancellationToken);
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
        return _executor.SendAsync<Superset[]>(
            new BricklinkRequest(ApiResourceType.Superset, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{itemNumber}/supersets")
                .WithQueryParameter("color_id", colorId > 0 ? colorId : null)
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Subset[]> GetSubsetsAsync(ItemType type, string itemNumber,
        int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null,
        bool? breakSubsets = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Subset[]>(
            new BricklinkRequest(ApiResourceType.Subset, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{itemNumber}/subsets")
                .WithQueryParameter("color_id", colorId)
                .WithQueryParameter("box", includeOriginalBox)
                .WithQueryParameter("instruction", includeInstruction)
                .WithQueryParameter("break_minifigs", breakMinifigs)
                .WithQueryParameter("break_subsets", breakSubsets)
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<PriceGuide> GetPriceGuideAsync(ItemType type, string itemNumber, int colorId = 0,
        PriceGuideSource? priceGuideType = null, Condition? condition = null,
        string? countryCode = null, string? region = null, string? currencyCode = null,
        VatOption? vat = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<PriceGuide>(
            new BricklinkRequest(ApiResourceType.PriceGuide, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{itemNumber}/price")
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

    public Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string itemNumber,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<KnownColor[]>(
            new BricklinkRequest(ApiResourceType.KnownColor, HttpVerb.Get,
                    $"items/{type.ToDomainString()}/{itemNumber}/colors")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Color[]> GetColorsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Color[]>(
            new BricklinkRequest(ApiResourceType.ColorList, HttpVerb.Get, "colors")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Color> GetColorAsync(int colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Color>(
            new BricklinkRequest(ApiResourceType.Color, HttpVerb.Get, $"colors/{colorId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Category[]> GetCategoriesAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Category[]>(
            new BricklinkRequest(ApiResourceType.CategoryList, HttpVerb.Get, "categories")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Category> GetCategoryAsync(int categoryId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Category>(
            new BricklinkRequest(ApiResourceType.Category, HttpVerb.Get, $"categories/{categoryId}")
                .WithCredentials(credentials),
            cancellationToken);
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
        return _executor.SendAsync<Inventory[]>(
            new BricklinkRequest(ApiResourceType.InventoryList, HttpVerb.Get, "inventories")
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
            new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Get, $"inventories/{inventoryId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        DtoValidation.Validate(newInventory);

        return await _executor.SendAsync<Inventory>(
            new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Post, "inventories")
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
            DtoValidation.Validate(newInventory);
        }

        await _executor.SendAsync<NoData>(
            new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Post, "inventories")
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
        DtoValidation.Validate(updateInventory);

        return await _executor.SendAsync<Inventory>(
            new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Put, $"inventories/{inventoryId}")
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
            new BricklinkRequest(ApiResourceType.Inventory, HttpVerb.Delete, $"inventories/{inventoryId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ItemMapping[]> GetItemMappingsByItemAsync(string itemNumber, int? colorId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ItemMapping[]>(
            new BricklinkRequest(ApiResourceType.ElementId, HttpVerb.Get, $"item_mapping/part/{itemNumber}")
                .WithQueryParameter("color_id", colorId)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ItemMapping[]> GetItemMappingsByElementIdAsync(string elementId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ItemMapping[]>(
            new BricklinkRequest(ApiResourceType.ItemNumber, HttpVerb.Get, $"item_mapping/{elementId}")
                .WithShape(ResponseShape.ArrayNonEmpty)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ShippingMethod[]> GetShippingMethodsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ShippingMethod[]>(
            new BricklinkRequest(ApiResourceType.ShippingMethod, HttpVerb.Get, "settings/shipping_methods")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<ShippingMethod> GetShippingMethodAsync(int shippingMethodId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<ShippingMethod>(
            new BricklinkRequest(ApiResourceType.ShippingMethod, HttpVerb.Get,
                    $"settings/shipping_methods/{shippingMethodId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<PushNotification[]> GetNotificationsAsync(BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<PushNotification[]>(
            new BricklinkRequest(ApiResourceType.Notification, HttpVerb.Get, "notifications")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<MemberRating> GetMemberRatingAsync(string username,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<MemberRating>(
            new BricklinkRequest(ApiResourceType.MemberRating, HttpVerb.Get, $"members/{username}/ratings")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback[]> GetFeedbacksAsync(Direction? direction = null,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback[]>(
            new BricklinkRequest(ApiResourceType.FeedbackList, HttpVerb.Get, "feedback")
                .WithQueryParameter("direction", direction?.ToString().ToLowerInvariant())
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback> GetFeedbackAsync(int feedbackId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback>(
            new BricklinkRequest(ApiResourceType.Feedback, HttpVerb.Get, $"feedback/{feedbackId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback> PostFeedbackAsync(int orderId,
        Rating rating,
        string comment,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        var body = new FeedbackPostBody
        {
            Comment = comment,
            OrderId = orderId,
            Rating = rating
        };

        return _executor.SendAsync<Feedback>(
            new BricklinkRequest(ApiResourceType.Feedback, HttpVerb.Post, "feedback")
                .WithBody(body)
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task ReplyToFeedbackAsync(int feedbackId,
        string reply,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(ApiResourceType.Feedback, HttpVerb.Post, $"feedback/{feedbackId}/reply")
                .WithBody(new { reply })
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Order[]> GetOrdersAsync(Direction direction = Direction.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Order[]>(
            new BricklinkRequest(ApiResourceType.Order, HttpVerb.Get, "orders")
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
            new BricklinkRequest(ApiResourceType.Order, HttpVerb.Get, $"orders/{orderId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        var itemsBatches = await _executor.SendAsync<OrderItem[][]>(
                new BricklinkRequest(ApiResourceType.OrderItem, HttpVerb.Get, $"orders/{orderId}/items")
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
            new BricklinkRequest(ApiResourceType.OrderMessage, HttpVerb.Get, $"orders/{orderId}/messages")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task<Feedback[]> GetOrderFeedbackAsync(int orderId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<Feedback[]>(
            new BricklinkRequest(ApiResourceType.OrderFeedback, HttpVerb.Get, $"orders/{orderId}/feedback")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task UpdateOrderStatusAsync(int orderId,
        OrderStatus status,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(ApiResourceType.OrderStatus, HttpVerb.Put, $"orders/{orderId}/status")
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
            new BricklinkRequest(ApiResourceType.PaymentStatus, HttpVerb.Put,
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
            new BricklinkRequest(ApiResourceType.DriveThru, HttpVerb.Post, $"orders/{orderId}/drive_thru")
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
            new BricklinkRequest(ApiResourceType.Order, HttpVerb.Put, $"orders/{orderId}")
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
            new BricklinkRequest(ApiResourceType.Coupon, HttpVerb.Get, "coupons")
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
            new BricklinkRequest(ApiResourceType.Coupon, HttpVerb.Get, $"coupons/{couponId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public Task DeleteCouponAsync(int couponId,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        return _executor.SendAsync<NoData>(
            new BricklinkRequest(ApiResourceType.Coupon, HttpVerb.Delete, $"coupons/{couponId}")
                .WithCredentials(credentials),
            cancellationToken);
    }

    public async Task<Coupon> CreateCouponAsync(NewCoupon newCoupon,
        BricklinkCredentials? credentials = null,
        CancellationToken cancellationToken = default)
    {
        DtoValidation.Validate(newCoupon);

        return await _executor.SendAsync<Coupon>(
            new BricklinkRequest(ApiResourceType.Coupon, HttpVerb.Post, "coupons")
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
        DtoValidation.Validate(updateCoupon);

        return await _executor.SendAsync<Coupon>(
            new BricklinkRequest(ApiResourceType.Coupon, HttpVerb.Put, $"coupons/{couponId}")
                .WithBody(updateCoupon)
                .WithCredentials(credentials),
            cancellationToken)
            .ConfigureAwait(false);
    }
}
