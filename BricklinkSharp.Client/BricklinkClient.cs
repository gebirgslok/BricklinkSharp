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
using System.Text.Json;
using System.Threading;
#if HAVE_JSON_DEFAULT_IGNORE_CONDITION
using System.Text.Json.Serialization;
#endif
using System.Threading.Tasks;
using System.Web;
using BricklinkSharp.Client.CurrencyRates;
using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client;

internal sealed class BricklinkClient : IBricklinkClient
{
    private static readonly string _partImageUrlTemplate = "//img.bricklink.com/ItemImage/PN/{0}/{1}.png";
    private static readonly string _minifigImageUrlTemplate = "//img.bricklink.com/ItemImage/MN/0/{0}.png";
    private static readonly string _setImageUrlTemplate = "//img.bricklink.com/ItemImage/SN/0/{0}.png";
    private static readonly string _bookImageUrlTemplate = "//img.bricklink.com/ItemImage/BN/0/{0}.png";
    private static readonly string _gearImageUrlTemplate = "//img.bricklink.com/ItemImage/GN/0/{0}.png";
    private static readonly string _catalogImageUrlTemplate = "//img.bricklink.com/ItemImage/CN/0/{0}.png";
    private static readonly string _instructionImageUrlTemplate = "//img.bricklink.com/ItemImage/IN/0/{0}.png";
    private static readonly string _originalBoxImageUrlTemplate = "//img.bricklink.com/ItemImage/ON/0/{0}.png";

    private readonly IExchangeRatesService _currencyRatesService;
    private static readonly Uri _baseUri = new("https://api.bricklink.com/api/store/v1/");
    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    private bool _isDisposed;

    public BricklinkClient(HttpClient httpClient, 
        IExchangeRatesService currencyRatesService, 
        bool disposeHttpClient)
    {
        _httpClient = httpClient;
        _currencyRatesService = currencyRatesService;
        _disposeHttpClient = disposeHttpClient;
    }

    ~BricklinkClient()
    {
        Dispose(false);
    }

    private static JsonSerializerOptions IgnoreNullValuesJsonSerializerOptions => new()
    {
#if HAVE_JSON_DEFAULT_IGNORE_CONDITION
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
        IgnoreNullValues = true,
#endif
    };

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

    public async Task<CatalogItem> GetItemAsync(ItemType type, string no, 
        CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();
        var url = new Uri(_baseUri, $"items/{typeString}/{no}").ToString();

        return await _httpClient.GetParseResponseAsync<CatalogItem>(url, 
            cancellationToken);
    }

    public async Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId, 
        CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();
        var url = new Uri(_baseUri, $"items/{typeString}/{no}/images/{colorId}").ToString();

        return await _httpClient.GetParseResponseAsync<CatalogImage>(url, 
            cancellationToken);
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

    public Uri EnsureImageUrlScheme(string imageUrl, string scheme = "https")
    {
        return new Uri($"{scheme}:{imageUrl}");
    }

    public async Task<Superset[]> GetSupersetsAsync(ItemType type, string no,
        int colorId = 0, CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();

        var url = new Uri(_baseUri,
                colorId > 0 ?
                    $"items/{typeString}/{no}/supersets?color_id={colorId}" :
                    $"items/{typeString}/{no}/supersets")
            .ToString();

        return await _httpClient.GetParseResponseAsync<Superset[]>(url, 
            cancellationToken);
    }

    public async Task<Subset[]> GetSubsetsAsync(ItemType type, string no, 
        int colorId = 0, bool? includeOriginalBox = null,
        bool? includeInstruction = null, bool? breakMinifigs = null, 
        bool? breakSubsets = null, CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();
        var builder = new UriBuilder(new Uri(_baseUri, $"items/{typeString}/{no}/subsets"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["color_id"] = colorId.ToString();
        query.AddIfNotNull("box", includeOriginalBox);
        query.AddIfNotNull("instruction", includeInstruction);
        query.AddIfNotNull("break_minifigs", breakMinifigs);
        query.AddIfNotNull("break_subsets", breakSubsets);
        builder.Query = query.ToString();

        var url = builder.ToString();

        return await _httpClient.GetParseResponseAsync<Subset[]>(url, 
            cancellationToken);
    }

    public async Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, int colorId = 0,
        PriceGuideType? priceGuideType = null, Condition? condition = null,
        string? countryCode = null, string? region = null, string? currencyCode = null,
        CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();
        var builder = new UriBuilder(new Uri(_baseUri, $"items/{typeString}/{no}/price"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query["color_id"] = colorId.ToString();
        query.AddIfNotNull("guide_type", priceGuideType?.ToDomainString());
        query.AddIfNotNull("new_or_used", condition?.ToDomainString());
        query.AddIfNotNull("country_code", countryCode);
        query.AddIfNotNull("region", region);
        query.AddIfNotNull("currency_code", currencyCode);
        builder.Query = query.ToString();

        var url = builder.ToString();

        return await _httpClient.GetParseResponseAsync<PriceGuide>(url, 
            cancellationToken);
    }

    public async Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no,
        CancellationToken cancellationToken = default)
    {
        var typeString = type.ToDomainString();
        var url = new Uri(_baseUri, $"items/{typeString}/{no}/colors").ToString();

        return await _httpClient.GetParseResponseAsync<KnownColor[]>(url, 
            cancellationToken);
    }

    public async Task<Color[]> GetColorListAsync(CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, "colors").ToString();

        return await _httpClient.GetParseResponseAsync<Color[]>(url, 
            cancellationToken);
    }

    public async Task<Color> GetColorAsync(int colorId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"colors/{colorId}").ToString();

        return await _httpClient.GetParseResponseAsync<Color>(url, 
            cancellationToken);
    }

    public async Task<Category[]> GetCategoryListAsync(CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, "categories").ToString();

        return await _httpClient.GetParseResponseAsync<Category[]>(url, 
            cancellationToken);
    }

    public async Task<Category> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"categories/{categoryId}").ToString();

        return await _httpClient.GetParseResponseAsync<Category>(url, 
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
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, "inventories"));
        var query = HttpUtility.ParseQueryString(builder.Query);

        query.AddIfNotNull("item_type", BuildIncludeExcludeParameter(includedItemTypes, 
            excludedItemTypes, t => t.ToDomainString()));

        query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedStatusFlags, 
            excludedStatusFlags, f => f.ToDomainString()));

        query.AddIfNotNull("category_id", BuildIncludeExcludeParameter(includedCategoryIds, 
            excludedCategoryIds, categoryId => categoryId.ToString()));

        query.AddIfNotNull("color_id", BuildIncludeExcludeParameter(includedColorIds, excludedColorIds, 
            colorId => colorId.ToString()));

        builder.Query = query.ToString();

        var url = builder.ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Inventory>(url, cancellationToken);
    }

    public async Task<Inventory> GetInventoryAsync(int inventoryId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

        return await _httpClient.GetParseResponseAsync<Inventory>(url, 
            cancellationToken);
    }

    public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory, 
        CancellationToken cancellationToken = default)
    {
        newInventory.ValidateThrowException();
        var url = new Uri(_baseUri, "inventories").ToString();

        return await _httpClient.PostThenReadResponseAsync<Inventory>(url, 
            newInventory, cancellationToken: cancellationToken);
    }

    public Task CreateInventoriesAsync(NewInventory[] newInventories, CancellationToken cancellationToken = default)
    {
        foreach (var newInventory in newInventories)
        {
            newInventory.ValidateThrowException();
        }

        var url = new Uri(_baseUri, "inventories").ToString();

        return _httpClient.PostEnsureNoResponseDataAsync(url, 
            newInventories,
            cancellationToken: cancellationToken);
    }

    public async Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdateInventory updateInventory,
        CancellationToken cancellationToken = default)
    {
        updateInventory.ValidateThrowException();
        var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

        return await _httpClient.PutThenReadResponseAsync<Inventory>(url,
            updateInventory,
            jsonSerializerOptions: IgnoreNullValuesJsonSerializerOptions,
            cancellationToken: cancellationToken);
    }

    public Task DeleteInventoryAsync(int inventoryId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

        return _httpClient.DeleteEnsureNoResponseDataAsync(url, cancellationToken: cancellationToken);
    }

    public Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId, 
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, $"item_mapping/part/{partNo}"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.AddIfNotNull("color_id", colorId);
        builder.Query = query.ToString();
        var url = builder.ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<ItemMapping>(url, cancellationToken);
    }

    public async Task<ItemMapping[]> GetItemNumberAsync(string elementId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"item_mapping/{elementId}").ToString();

        return await _httpClient.GetParseResponseAsync<ItemMapping[]>(url,
            cancellationToken: cancellationToken);
    }

    public Task<ShippingMethod[]> GetShippingMethodListAsync(CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, "settings/shipping_methods").ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<ShippingMethod>(url,
            cancellationToken: cancellationToken);
    }

    public async Task<ShippingMethod> GetShippingMethodAsync(int methodId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"settings/shipping_methods/{methodId}").ToString();

        return await _httpClient.GetParseResponseAsync<ShippingMethod>(url, 
            cancellationToken);
    }

    public Task<Notification[]> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, "notifications").ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Notification>(url,
            cancellationToken: cancellationToken);
    }

    public async Task<MemberRating> GetMemberRatingAsync(string username, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"members/{username}/ratings").ToString();

        return await _httpClient.GetParseResponseAsync<MemberRating>(url,
            cancellationToken: cancellationToken);
    }

    public Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null, 
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, "feedback"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.AddIfNotNull("direction", direction, d => d!.ToString()!.ToLowerInvariant());
        builder.Query = query.ToString();

        var url = builder.ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Feedback>(url, cancellationToken);
    }

    public async Task<Feedback> GetFeedbackAsync(int feedbackId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"feedback/{feedbackId}").ToString();

        return await _httpClient.GetParseResponseAsync<Feedback>(url, 
            cancellationToken);
    }

    public async Task<Feedback> PostFeedbackAsync(int orderId, RatingType rating, string comment,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, "feedback").ToString();
        var body = new FeedbackBase
        {
            Comment = comment,
            OrderId = orderId,
            Rating = rating
        };

        return await _httpClient.PostThenReadResponseAsync<Feedback>(url, body,
            cancellationToken: cancellationToken);
    }

    public Task ReplyFeedbackAsync(int feedbackId, 
        string reply, 
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"feedback/{feedbackId}/reply").ToString();
        var body = new { reply };

        return _httpClient.PostEnsureNoResponseDataAsync(url, body, cancellationToken: cancellationToken);
    }

    public Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
        IEnumerable<OrderStatus>? includedStatusFlags = null,
        IEnumerable<OrderStatus>? excludedStatusFlags = null,
        bool filed = false,
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, "orders"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("direction", direction.ToDomainString());
        query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedStatusFlags, excludedStatusFlags, f => f.ToDomainString()));
        query.Add("filed", filed.ToString());
        builder.Query = query.ToString();

        var url = builder.ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Order>(url, cancellationToken);
    }

    public async Task<OrderDetails> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

        return await _httpClient.GetParseResponseAsync<OrderDetails>(url, 
            cancellationToken);
    }

    public async Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}/items").ToString();

        var method = HttpMethod.Get;

        var responseBody = await _httpClient.ExecuteRequestAsync(url, method, 
            cancellationToken: cancellationToken);

        var itemsBatchList = new List<OrderItem[]>();
        using var document = JsonDocument.Parse(responseBody);
        var dataElement = document.GetData(200, url, method);

        if (dataElement.ValueKind != JsonValueKind.Array)
        {
            throw new BricklinkUnexpectedDataKindException(JsonValueKind.Array.ToString(), 
                dataElement.ValueKind.ToString(),
                url, 
                HttpMethod.Get);
        }

        foreach (var innerList in dataElement.EnumerateArray())
        {
            itemsBatchList.Add(innerList.ToObject<OrderItem[]>());
        }

        return itemsBatchList;
    }

    public Task<OrderMessage[]> GetOrderMessagesAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}/messages").ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<OrderMessage>(url, 
            cancellationToken);
    }

    public Task<Feedback[]> GetOrderFeedbackAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}/feedback").ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Feedback>(url, 
            cancellationToken);
    }

    public Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}/status").ToString();

        return _httpClient.PutEnsureNoResponseDataAsync(url, new
        {
            field = "status",
            value = status.ToDomainString()
        }, cancellationToken: cancellationToken);
    }

    public Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status, 
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}/payment_status").ToString();

        return _httpClient.PutEnsureNoResponseDataAsync(url, new
        {
            field = "payment_status",
            value = status.ToDomainString()
        }, cancellationToken: cancellationToken);
    }

    public Task SendDriveThruAsync(int orderId, bool mailCcMe,
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, $"orders/{orderId}/drive_thru"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("mail_me", mailCcMe.ToString());
        builder.Query = query.ToString();

        var url = builder.ToString();

        return _httpClient.PutEnsureNoResponseDataAsync(url, cancellationToken: cancellationToken);
    }

    public async Task<OrderDetails> UpdateOrderAsync(int orderId, UpdateOrder updateOrder,
        CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

        return await _httpClient.PutThenReadResponseAsync<OrderDetails>(url, updateOrder,
            jsonSerializerOptions: IgnoreNullValuesJsonSerializerOptions,
            cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
        IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
        IEnumerable<CouponStatus>? excludedCouponStatusTypes = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(new Uri(_baseUri, "coupons"));
        var query = HttpUtility.ParseQueryString(builder.Query);
        query.Add("direction", direction.ToDomainString());

        query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedCouponStatusTypes,
            excludedCouponStatusTypes,
            f => f.ToDomainString()));

        builder.Query = query.ToString();

        var url = builder.ToString();

        return _httpClient.GetParseResponseArrayAllowEmpty<Coupon>(url, 
            cancellationToken);
    }

    public async Task<Coupon> GetCouponAsync(int couponId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();

        return await _httpClient.GetParseResponseAsync<Coupon>(url, 
            cancellationToken);
    }

    public Task DeleteCouponAsync(int couponId, CancellationToken cancellationToken = default)
    {
        var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();

        return _httpClient.DeleteEnsureNoResponseDataAsync(url,
            cancellationToken: cancellationToken);
    }

    public async Task<Coupon> CreateCouponAsync(NewCoupon newCoupon, CancellationToken cancellationToken = default)
    {
        newCoupon.ValidateThrowException();

        var url = new Uri(_baseUri, "coupons").ToString();

        return await _httpClient.PostThenReadResponseAsync<Coupon>(url, newCoupon,
            cancellationToken: cancellationToken);
    }

    public async Task<Coupon> UpdateCouponAsync(int couponId, UpdateCoupon updateCoupon, 
        CancellationToken cancellationToken = default)
    {
        updateCoupon.ValidateThrowException();

        var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();

        return await _httpClient.PutThenReadResponseAsync<Coupon>(url,
            updateCoupon,
            jsonSerializerOptions: IgnoreNullValuesJsonSerializerOptions,
            cancellationToken: cancellationToken);
    }

    public async Task<PartOutResult> GetPartOutValueFromPageAsync(string itemNo, Condition condition = Condition.New,
        bool includeInstructions = true, bool breakMinifigs = false, bool includeBox = false,
        bool includeExtraParts = false, bool breakSetsInSet = false, PartOutItemType itemType = PartOutItemType.Set,
        string? currencyCode = null, CancellationToken cancellationToken = default)
    {
        var split = itemNo.Split('-');
        var rawItemNumber = split[0];

        var sequenceNumber = 1;

        if (split.Length > 1)
        {
            if (!int.TryParse(split[1], out int temp))
            {
                throw new ArgumentException($"{itemNo} '{itemNo}' has an invalid sequence number. " +
                                            "The section after the '-' must be numeric.",
                    nameof(itemNo));
            }

            sequenceNumber = temp;
        }

        var url = "https://www.bricklink.com/catalogPOV.asp?" +
                  $"itemType={itemType.ToDomainString()}&itemNo={rawItemNumber}&itemSeq={sequenceNumber}&itemQty=1&" +
                  $"breakType={(breakMinifigs ? "P" : "M")}&itemCondition={condition.ToDomainString()}" +
                  $"&incInstr={(includeInstructions ? "Y" : "N")}&incBox={(includeBox ? "Y" : "N")}&" +
                  $"incParts={(includeExtraParts ? "Y" : "N")}&breakSets={(breakSetsInSet ? "Y" : "N")}";

        var response = await _httpClient.GetAsync(url, cancellationToken);

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
            var htmlResponse = await response.Content.ReadAsStringAsync(cancellationToken);
#else
        var htmlResponse = await response.Content.ReadAsStringAsync();
#endif

        var result = PartOutResponseParser.ParseResponse(htmlResponse, url);
        result.ItemNumber = $"{rawItemNumber}-{sequenceNumber}";
        result.Condition = condition;

        if (currencyCode == null)
        {
            return result;
        }

        var exchangeRate = await _currencyRatesService
            .GetExchangeRateAsync("USD", currencyCode, cancellationToken);

        result.Average6MonthsSalesValueMyCurrency = Math.Round(result.Average6MonthsSalesValueUsd * exchangeRate, 3);
        result.CurrentSalesValueMyCurreny = Math.Round(result.CurrentSalesValueUsd * exchangeRate, 3);

        return result;
    }
}