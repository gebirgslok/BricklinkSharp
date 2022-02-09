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
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
#if HAVE_JSON_DEFAULT_IGNORE_CONDITION
using System.Text.Json.Serialization;
#endif
using System.Threading.Tasks;
using System.Web;
using BricklinkSharp.Client.CurrencyRates;
using BricklinkSharp.Client.Extensions;
using BricklinkSharp.Client.OAuth;

#pragma warning disable 8602

namespace BricklinkSharp.Client
{
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

        public BricklinkClient(HttpClient httpClient, IExchangeRatesService currencyRatesService, bool disposeHttpClient)
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

        private static void GetAuthorizationHeader(string url, string method, out string scheme, out string parameter)
        {
            BricklinkClientConfiguration.Instance.ValidateThrowException();

            var request = new OAuthRequest(BricklinkClientConfiguration.Instance.ConsumerKey!,
                BricklinkClientConfiguration.Instance.ConsumerSecret!,
                BricklinkClientConfiguration.Instance.TokenValue!,
                BricklinkClientConfiguration.Instance.TokenSecret!,
                url,
                method);

            var header = request.GetAuthorizationHeader();
            var schemeParameter = header.Split(' ');
            scheme = schemeParameter[0];
            parameter = schemeParameter[1];
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

        private async Task<string> ExecuteRequest<TBody>(string url, HttpMethod httpMethod,
            TBody body, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            GetAuthorizationHeader(url, httpMethod.ToString(), out var authScheme, out var authParameter);
            using var message = new HttpRequestMessage(httpMethod, url);
            message.Headers.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);
            var json = JsonSerializer.Serialize(body, options);
            using var content = new StringContent(json, Encoding.Default, "application/json");
            content.Headers.ContentType.CharSet = string.Empty;
            message.Content = content;

            var response = await _httpClient.SendAsync(message, cancellationToken);

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
            var contentAsString = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var contentAsString = await response.Content.ReadAsStringAsync();
#endif
            return contentAsString;
        }

        private async Task<string> ExecuteRequest(string url, HttpMethod httpMethod, CancellationToken cancellationToken = default)
        {
            GetAuthorizationHeader(url, httpMethod.ToString(), out var authScheme, out var authParameter);
            using var message = new HttpRequestMessage(httpMethod, url);
            message.Headers.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);
            message.Content = null;

            var response = await _httpClient.SendAsync(message, cancellationToken);

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
            var contentAsString = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var contentAsString = await response.Content.ReadAsStringAsync();
#endif

            return contentAsString;
        }

        private static JsonElement GetData(JsonDocument document, int expectedCode, string url, HttpMethod httpMethod)
        {
            var meta = document.RootElement.GetProperty("meta").ToObject<ResponseMeta>();

            if (meta.Code != expectedCode)
            {
                throw new BricklinkHttpErrorException(meta.Code, expectedCode, meta.Description, meta.Message, url, httpMethod);
            }

            document.RootElement.TryGetProperty("data", out var dataElement);
            return dataElement;
        }

        private static TData[] ParseResponseArrayAllowEmpty<TData>(string responseBody, int expectedCode, string url,
            HttpMethod httpMethod)
        {
            using var document = JsonDocument.Parse(responseBody);
            var dataElement = GetData(document, expectedCode, url, httpMethod);

            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new BricklinkUnexpectedDataKindException(JsonValueKind.Array.ToString(), dataElement.ValueKind.ToString(),
                    url, httpMethod);
            }

            var dataArray = dataElement.ToObject<TData[]>();
            return dataArray;
        }

        private static void ParseResponseNoData(string responseBody, int expectedCode, string url,
            HttpMethod httpMethod)
        {
            using var document = JsonDocument.Parse(responseBody);
            var dataElement = GetData(document, expectedCode, url, httpMethod);

            if (!(dataElement.ValueKind == JsonValueKind.Object || dataElement.ValueKind == JsonValueKind.Undefined))
            {
                throw new BricklinkUnexpectedDataKindException(JsonValueKind.Object.ToString(), dataElement.ValueKind.ToString(),
                    url, httpMethod);
            }

            if (!dataElement.IsEmpty())
            {
                throw new BricklinkEmptyDataExpectedException(dataElement, url, httpMethod);
            }
        }

        private static TData ParseResponse<TData>(string responseBody, int expectedCode, string url, HttpMethod httpMethod)
        {
            using var document = JsonDocument.Parse(responseBody);
            var dataElement = GetData(document, expectedCode, url, httpMethod);

            if (dataElement.IsEmpty())
            {
                throw new BricklinkNoDataReceivedException(url, httpMethod);
            }

            var data = dataElement.ToObject<TData>();
            return data;
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

        public async Task<CatalogItem> GetItemAsync(ItemType type, string no, CancellationToken cancellationToken)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            var data = ParseResponse<CatalogItem>(responseBody, 200, url, method);
            return data;
        }

        public async Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId, CancellationToken cancellationToken)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/images/{colorId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            var data = ParseResponse<CatalogImage>(responseBody, 200, url, method);
            return data;
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
            var typeString = type.GetStringValueOrDefault();

            var url = new Uri(_baseUri,
                colorId > 0 ?
                    $"items/{typeString}/{no}/supersets?color_id={colorId}" :
                    $"items/{typeString}/{no}/supersets")
                .ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            var data = ParseResponse<Superset[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
            bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null,
            CancellationToken cancellationToken = default)
        {
            var typeString = type.GetStringValueOrDefault();
            var builder = new UriBuilder(new Uri(_baseUri, $"items/{typeString}/{no}/subsets"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["color_id"] = colorId.ToString();
            query.AddIfNotNull("box", includeOriginalBox);
            query.AddIfNotNull("instruction", includeInstruction);
            query.AddIfNotNull("break_minifigs", breakMinifigs);
            query.AddIfNotNull("break_subsets", breakSubsets);
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(builder.ToString(), method, cancellationToken);

            var data = ParseResponse<Subset[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, int colorId = 0,
            PriceGuideType? priceGuideType = null, Condition? condition = null,
            string? countryCode = null, string? region = null, string? currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            var typeString = type.GetStringValueOrDefault();
            var builder = new UriBuilder(new Uri(_baseUri, $"items/{typeString}/{no}/price"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["color_id"] = colorId.ToString();
            query.AddIfNotNull("guide_type", priceGuideType, pg => pg!.ToString().ToLower());
            query.AddIfNotNull("new_or_used", condition, c => c!.GetStringValueOrDefault());

            query.AddIfNotNull("country_code", countryCode);
            query.AddIfNotNull("region", region);
            query.AddIfNotNull("currency_code", currencyCode);
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(builder.ToString(), method, cancellationToken);

            var data = ParseResponse<PriceGuide>(responseBody, 200, url, method);
            return data;
        }

        public async Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no,
            CancellationToken cancellationToken = default)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/colors").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            var data = ParseResponse<KnownColor[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Color[]> GetColorListAsync(CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "colors").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            var data = ParseResponse<Color[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Color> GetColorAsync(int colorId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"colors/{colorId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Color>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Category[]> GetCategoryListAsync(CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "categories").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Category[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Category> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"categories/{categoryId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Category>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType>? includedItemTypes = null,
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
            query.AddIfNotNull("item_type", BuildIncludeExcludeParameter(includedItemTypes, excludedItemTypes, t => t.GetStringValueOrDefault()));
            query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedStatusFlags, excludedStatusFlags, f => f.GetStringValueOrDefault()));
            query.AddIfNotNull("category_id", BuildIncludeExcludeParameter(includedCategoryIds, excludedCategoryIds, categoryId => categoryId.ToString()));
            query.AddIfNotNull("color_id", BuildIncludeExcludeParameter(includedColorIds, excludedColorIds, colorId => colorId.ToString()));
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var dataArray = ParseResponseArrayAllowEmpty<Inventory>(responseBody, 200, url, method);
            return dataArray;
        }

        public async Task<Inventory> GetInventoryAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Inventory>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory, CancellationToken cancellationToken = default)
        {
            newInventory.ValidateThrowException();
            var url = new Uri(_baseUri, "inventories").ToString();

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, newInventory, cancellationToken: cancellationToken);
            var data = ParseResponse<Inventory>(responseBody, 201, url, method);
            return data;
        }

        public async Task CreateInventoriesAsync(NewInventory[] newInventories, CancellationToken cancellationToken = default)
        {
            foreach (var newInventory in newInventories)
            {
                newInventory.ValidateThrowException();
            }

            var url = new Uri(_baseUri, "inventories").ToString();
            var method = HttpMethod.Post;

            var responseBody = await ExecuteRequest(url, method, newInventories, cancellationToken: cancellationToken);
            ParseResponseNoData(responseBody, 201, url, HttpMethod.Post);
        }

        public async Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdateInventory updatedInventory,
            CancellationToken cancellationToken = default)
        {
            updatedInventory.ValidateThrowException();
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, updatedInventory, IgnoreNullValuesJsonSerializerOptions, cancellationToken);

            var data = ParseResponse<Inventory>(responseBody, 200, url, method);
            return data;
        }

        public async Task DeleteInventoryAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();
            var responseBody = await ExecuteRequest(url, HttpMethod.Delete, cancellationToken);
            ParseResponseNoData(responseBody, 204, url, HttpMethod.Delete);
        }

        public async Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId, 
            CancellationToken cancellationToken = default)
        {
            var builder = new UriBuilder(new Uri(_baseUri, $"item_mapping/part/{partNo}"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.AddIfNotNull("color_id", colorId);
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var itemMappings = ParseResponseArrayAllowEmpty<ItemMapping>(responseBody, 200, url, method);
            return itemMappings;
        }

        public async Task<ItemMapping[]> GetItemNumberAsync(string elementId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"item_mapping/{elementId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var itemMappings = ParseResponse<ItemMapping[]>(responseBody, 200, url, method);
            return itemMappings;
        }

        public async Task<ShippingMethod[]> GetShippingMethodListAsync(CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "settings/shipping_methods").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponseArrayAllowEmpty<ShippingMethod>(responseBody, 200, url, method);
            return data;
        }

        public async Task<ShippingMethod> GetShippingMethodAsync(int methodId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"settings/shipping_methods/{methodId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<ShippingMethod>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Notification[]> GetNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "notifications").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponseArrayAllowEmpty<Notification>(responseBody, 200, url, method);
            return data;
        }

        public async Task<MemberRating> GetMemberRatingAsync(string username, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"members/{username}/ratings").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<MemberRating>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Feedback[]> GetFeedbackListAsync(Direction? direction = null, 
            CancellationToken cancellationToken = default)
        {
            var builder = new UriBuilder(new Uri(_baseUri, "feedback"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.AddIfNotNull("direction", direction, d => d!.ToString().ToLowerInvariant());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponseArrayAllowEmpty<Feedback>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Feedback> GetFeedbackAsync(int feedbackId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"feedback/{feedbackId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Feedback>(responseBody, 200, url, method);
            return data;
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

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, body, cancellationToken: cancellationToken);
            var data = ParseResponse<Feedback>(responseBody, 201, url, method);
            return data;
        }

        public async Task ReplyFeedbackAsync(int feedbackId, string reply, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"feedback/{feedbackId}/reply").ToString();
            var body = new { reply };

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, body, cancellationToken: cancellationToken);
            ParseResponseNoData(responseBody, 201, url, method);
        }

        public async Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
            IEnumerable<OrderStatus>? includedStatusFlags = null,
            IEnumerable<OrderStatus>? excludedStatusFlags = null,
            bool filed = false,
            CancellationToken cancellationToken = default)
        {
            var builder = new UriBuilder(new Uri(_baseUri, "orders"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("direction", direction.GetStringValueOrDefault());
            query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedStatusFlags, excludedStatusFlags, f => f.GetStringValueOrDefault()));
            query.Add("filed", filed.ToString());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var orders = ParseResponseArrayAllowEmpty<Order>(responseBody, 200, url, method);
            return orders;
        }

        public async Task<OrderDetails> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<OrderDetails>(responseBody, 200, url, method);
            return data;
        }

        public async Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/items").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var itemsBatchList = new List<OrderItem[]>();
            using var document = JsonDocument.Parse(responseBody);
            var dataElement = GetData(document, 200, url, method);

            if (dataElement.ValueKind != JsonValueKind.Array)
            {
                throw new BricklinkUnexpectedDataKindException(JsonValueKind.Array.ToString(), dataElement.ValueKind.ToString(),
                    url, HttpMethod.Get);
            }

            foreach (var innerList in dataElement.EnumerateArray())
            {
                itemsBatchList.Add(innerList.ToObject<OrderItem[]>());
            }

            return itemsBatchList;
        }

        public async Task<OrderMessage[]> GetOrderMessagesAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/messages").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var messages = ParseResponseArrayAllowEmpty<OrderMessage>(responseBody, 200, url, method);
            return messages;
        }

        public async Task<Feedback[]> GetOrderFeedbackAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/feedback").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var feedbackArray = ParseResponseArrayAllowEmpty<Feedback>(responseBody, 200, url, method);
            return feedbackArray;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/status").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, new
            {
                field = "status",
                value = status.GetStringValueOrDefault()
            }, cancellationToken: cancellationToken);

            ParseResponseNoData(responseBody, 200, url, method);
        }

        public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status, 
            CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/payment_status").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, new
            {
                field = "payment_status",
                value = status.ToString()
            }, cancellationToken: cancellationToken);

            ParseResponseNoData(responseBody, 200, url, HttpMethod.Put);
        }

        public async Task SendDriveThruAsync(int orderId, bool mailCcMe,
            CancellationToken cancellationToken = default)
        {
            var builder = new UriBuilder(new Uri(_baseUri, $"orders/{orderId}/drive_thru"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("mail_me", mailCcMe.ToString());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);

            ParseResponseNoData(responseBody, 200, url, method);
        }

        public async Task<OrderDetails> UpdateOrderAsync(int orderId, UpdateOrder updateOrder,
            CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, updateOrder, 
                IgnoreNullValuesJsonSerializerOptions, cancellationToken);

            var order = ParseResponse<OrderDetails>(responseBody, 200, url, method);
            return order;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Coupon[]> GetCouponsAsync(Direction direction = Direction.Out,
            IEnumerable<CouponStatus>? includedCouponStatusTypes = null,
            IEnumerable<CouponStatus>? excludedCouponStatusTypes = null,
            CancellationToken cancellationToken = default)
        {
            var builder = new UriBuilder(new Uri(_baseUri, "coupons"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("direction", direction.GetStringValueOrDefault());

            query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedCouponStatusTypes,
                excludedCouponStatusTypes,
                f => f.GetStringValueOrDefault()));

            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var coupons = ParseResponseArrayAllowEmpty<Coupon>(responseBody, 200, url, method);
            return coupons;
        }

        public async Task<Coupon> GetCouponAsync(int couponId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            var data = ParseResponse<Coupon>(responseBody, 200, url, method);
            return data;
        }

        public async Task DeleteCouponAsync(int couponId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();
            var method = HttpMethod.Delete;
            var responseBody = await ExecuteRequest(url, method, cancellationToken);
            ParseResponseNoData(responseBody, 204, url, method);
        }

        public async Task<Coupon> CreateCouponAsync(NewCoupon newCoupon, CancellationToken cancellationToken = default)
        {
            newCoupon.ValidateThrowException();

            var url = new Uri(_baseUri, "coupons").ToString();
            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, newCoupon, IgnoreNullValuesJsonSerializerOptions, cancellationToken);
            var coupon = ParseResponse<Coupon>(responseBody, 201, url, method);
            return coupon;
        }

        public async Task<Coupon> UpdateCouponAsync(int couponId, UpdateCoupon updateCoupon, 
            CancellationToken cancellationToken = default)
        {
            updateCoupon.ValidateThrowException();

            var url = new Uri(_baseUri, $"coupons/{couponId}").ToString();
            var method = HttpMethod.Put;

            var responseBody = await ExecuteRequest(url, method, updateCoupon, 
                IgnoreNullValuesJsonSerializerOptions, cancellationToken);

            var coupon = ParseResponse<Coupon>(responseBody, 200, url, method);
            return coupon;
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
                    throw new ArgumentException($"{itemNo} '{itemNo}' has an invalid sequence number. The section after the '-' must be numeric.",
                        nameof(itemNo));
                }

                sequenceNumber = temp;
            }

            var url = "https://www.bricklink.com/catalogPOV.asp?" +
                      $"itemType={itemType.GetStringValueOrDefault()}&itemNo={rawItemNumber}&itemSeq={sequenceNumber}&itemQty=1&" +
                      $"breakType={(breakMinifigs ? "P" : "M")}&itemCondition={condition.GetStringValueOrDefault()}" +
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

            if (currencyCode != null)
            {
                var exchangeRate = await _currencyRatesService.GetExchangeRateAsync("USD", currencyCode);

                result.Average6MonthsSalesValueMyCurrency = Math.Round(result.Average6MonthsSalesValueUsd * exchangeRate, 3);
                result.CurrentSalesValueMyCurreny = Math.Round(result.CurrentSalesValueUsd * exchangeRate, 3);
            }

            return result;
        }
    }
}
