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
using System.Threading.Tasks;
using System.Web;
using BricklinkSharp.Client.Extensions;
using BricklinkSharp.Client.OAuth;

namespace BricklinkSharp.Client
{
    internal sealed class BricklinkClient : IBricklinkClient
    {
        private static readonly Uri _baseUri = new Uri("https://api.bricklink.com/api/store/v1/");
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isDisposed;

        ~BricklinkClient()
        {
            Dispose(false);
        }

        private void GetAuthorizationHeader(string url, string method, out string scheme, out string parameter)
        {
            BricklinkClientConfiguration.Instance.ValidateThrowException();

            var request = new OAuthRequest(BricklinkClientConfiguration.Instance.ConsumerKey,
                BricklinkClientConfiguration.Instance.ConsumerSecret,
                BricklinkClientConfiguration.Instance.TokenValue,
                BricklinkClientConfiguration.Instance.TokenSecret,
                url,
                method);

            var header = request.GetAuthorizationHeader();
            var schemeParameter = header.Split(' ');
            scheme = schemeParameter[0];
            parameter = schemeParameter[1];
        }

        private string BuildIncludeExcludeParameter<T>(IEnumerable<T> includes, IEnumerable<T> excludes,
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
            TBody body, JsonSerializerOptions options = null)
        {
            GetAuthorizationHeader(url, httpMethod.ToString(), out var authScheme, out var authParameter);
            using var message = new HttpRequestMessage(httpMethod, url);
            message.Headers.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);
            var json = JsonSerializer.Serialize(body, options);
            using var content = new StringContent(json, Encoding.Default, "application/json");
            content.Headers.ContentType.CharSet = string.Empty;
            message.Content = content;

            var response = await _httpClient.SendAsync(message);

            var contentAsString = await response.Content.ReadAsStringAsync();
            return contentAsString;
        }

        private async Task<string> ExecuteRequest(string url, HttpMethod httpMethod)
        {
            GetAuthorizationHeader(url, httpMethod.ToString(), out var authScheme, out var authParameter);
            using var message = new HttpRequestMessage(httpMethod, url);
            message.Headers.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);
            message.Content = null;

            var response = await _httpClient.SendAsync(message);

            var contentAsString = await response.Content.ReadAsStringAsync();
            return contentAsString;
        }

        private JsonElement GetData(JsonDocument document, int expectedCode, string url, HttpMethod httpMethod)
        {
            var meta = document.RootElement.GetProperty("meta").ToObject<ResponseMeta>();

            if (meta.Code != expectedCode)
            {
                throw new BricklinkHttpErrorException(meta.Code, expectedCode, meta.Description, meta.Message, url, httpMethod);
            }

            document.RootElement.TryGetProperty("data", out var dataElement);
            return dataElement;
        }

        private TData[] ParseResponseArrayAllowEmpty<TData>(string responseBody, int expectedCode, string url,
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

        private void ParseResponseNoData(string responseBody, int expectedCode, string url,
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

        private TData ParseResponse<TData>(string responseBody, int expectedCode, string url, HttpMethod httpMethod)
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
                _httpClient.Dispose();
            }

            _isDisposed = true;
        }

        public async Task<CatalogItem> GetItemAsync(ItemType type, string no)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);

            var data = ParseResponse<CatalogItem>(responseBody, 200, url, method);
            return data;
        }

        public async Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/images/{colorId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);

            var data = ParseResponse<CatalogImage>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Superset[]> GetSupersetsAsync(ItemType type, string no, int colorId)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/supersets/{colorId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);

            var data = ParseResponse<Superset[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
            bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null)
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
            var responseBody = await ExecuteRequest(builder.ToString(), method);

            var data = ParseResponse<Subset[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, int colorId = 0,
            PriceGuideType? priceGuideType = null, Condition? condition = null, string countryCode = null,
            string region = null, string currencyCode = null)
        {
            var typeString = type.GetStringValueOrDefault();
            var builder = new UriBuilder(new Uri(_baseUri, $"items/{typeString}/{no}/price"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["color_id"] = colorId.ToString();
            query.AddIfNotNull("guide_type", priceGuideType, pg => pg.ToString().ToLower());
            query.AddIfNotNull("new_or_used", condition, c => c.GetStringValueOrDefault());
            query.AddIfNotNull("country_code", countryCode);
            query.AddIfNotNull("region", region);
            query.AddIfNotNull("currency_code", currencyCode);
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(builder.ToString(), method);

            var data = ParseResponse<PriceGuide>(responseBody, 200, url, method);
            return data;
        }

        public async Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/colors").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);

            var data = ParseResponse<KnownColor[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Color[]> GetColorListAsync()
        {
            var url = new Uri(_baseUri, "colors").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);

            var data = ParseResponse<Color[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Color> GetColorAsync(int colorId)
        {
            var url = new Uri(_baseUri, $"colors/{colorId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<Color>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Category[]> GetCategoryListAsync()
        {
            var url = new Uri(_baseUri, "categories").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<Category[]>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            var url = new Uri(_baseUri, $"categories/{categoryId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<Category>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType> includedItemTypes = null,
            IEnumerable<ItemType> excludedItemTypes = null,
            IEnumerable<InventoryStatusType> includedStatusFlags = null,
            IEnumerable<InventoryStatusType> excludedStatusFlags = null,
            IEnumerable<int> includedCategoryIds = null,
            IEnumerable<int> excludedCategoryIds = null,
            IEnumerable<int> includedColorIds = null,
            IEnumerable<int> excludedColorIds = null)
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
            var responseBody = await ExecuteRequest(url, method);
            var dataArray = ParseResponseArrayAllowEmpty<Inventory>(responseBody, 200, url, method);
            return dataArray;
        }

        public async Task<Inventory> GetInventoryAsync(int inventoryId)
        {
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<Inventory>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory)
        {
            newInventory.ValidateThrowException();
            var url = new Uri(_baseUri, "inventories").ToString();

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, newInventory);
            var data = ParseResponse<Inventory>(responseBody, 201, url, method);
            return data;
        }

        public async Task CreateInventoriesAsync(NewInventory[] newInventories)
        {
            foreach (var newInventory in newInventories)
            {
                newInventory.ValidateThrowException();
            }

            var url = new Uri(_baseUri, "inventories").ToString();
            var method = HttpMethod.Post;

            var responseBody = await ExecuteRequest(url, method, newInventories);
            ParseResponseNoData(responseBody, 201, url, HttpMethod.Post);
        }

        public async Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdatedInventory updatedInventory)
        {
            updatedInventory.ValidateThrowException();
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, updatedInventory, new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });

            var data = ParseResponse<Inventory>(responseBody, 200, url, method);
            return data;
        }

        public async Task DeleteInventoryAsync(int inventoryId)
        {
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();
            var responseBody = await ExecuteRequest(url, HttpMethod.Delete);
            ParseResponseNoData(responseBody, 204, url, HttpMethod.Delete);
        }

        public async Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId)
        {
            var builder = new UriBuilder(new Uri(_baseUri, $"item_mapping/PART/{partNo}"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.AddIfNotNull("color_id", colorId);
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var itemMappings = ParseResponse<ItemMapping[]>(responseBody, 200, url, method);
            return itemMappings;
        }

        public async Task<ItemMapping[]> GetItemNumberAsync(string elementId)
        {
            var url = new Uri(_baseUri, $"item_mapping/{elementId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var itemMappings = ParseResponse<ItemMapping[]>(responseBody, 200, url, method);
            return itemMappings;
        }

        public async Task<ShippingMethod[]> GetShippingMethodListAsync()
        {
            var url = new Uri(_baseUri, "settings/shipping_methods").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponseArrayAllowEmpty<ShippingMethod>(responseBody, 200, url, method);
            return data;
        }

        public async Task<ShippingMethod> GetShippingMethodAsync(int methodId)
        {
            var url = new Uri(_baseUri, $"settings/shipping_methods/{methodId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<ShippingMethod>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Notification[]> GetNotificationsAsync()
        {
            var url = new Uri(_baseUri, "notifications").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponseArrayAllowEmpty<Notification>(responseBody, 200, url, method);
            return data;
        }

        public async Task<MemberRating> GetMemberRatingAsync(string username)
        {
            var url = new Uri(_baseUri, $"members/{username}/ratings").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<MemberRating>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Feedback[]> GetFeedbackListAsync(FeedbackDirection? direction = null)
        {
            var builder = new UriBuilder(new Uri(_baseUri, "feedback"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.AddIfNotNull("direction", direction, d => d.ToString().ToLowerInvariant());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponseArrayAllowEmpty<Feedback>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Feedback> GetFeedbackAsync(int feedbackId)
        {
            var url = new Uri(_baseUri, $"feedback/{feedbackId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<Feedback>(responseBody, 200, url, method);
            return data;
        }

        public async Task<Feedback> PostFeedbackAsync(int orderId, RatingType rating, string comment)
        {
            var url = new Uri(_baseUri, "feedback").ToString();
            var body = new FeedbackBase
            {
                Comment = comment,
                OrderId = orderId,
                Rating = rating
            };

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, body);
            var data = ParseResponse<Feedback>(responseBody, 201, url, method);
            return data;
        }

        public async Task ReplyFeedbackAsync(int feedbackId, string reply)
        {
            var url = new Uri(_baseUri, $"feedback/{feedbackId}/reply").ToString();
            var body = new { reply };

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method, body);
            ParseResponseNoData(responseBody, 201, url, method);
        }

        public async Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
            IEnumerable<OrderStatus> includedStatusFlags = null,
            IEnumerable<OrderStatus> excludedStatusFlags = null,
            bool filed = false)
        {
            var builder = new UriBuilder(new Uri(_baseUri, "orders"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("direction", direction.GetStringValueOrDefault());
            query.AddIfNotNull("status", BuildIncludeExcludeParameter(includedStatusFlags, excludedStatusFlags, f => f.GetStringValueOrDefault()));
            query.Add("filed", filed.ToString());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var orders = ParseResponseArrayAllowEmpty<Order>(responseBody, 200, url, method);
            return orders;
        }

        public async Task<OrderDetails> GetOrderAsync(int orderId)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var data = ParseResponse<OrderDetails>(responseBody, 200, url, method);
            return data;
        }

        public async Task<List<OrderItem[]>> GetOrderItemsAsync(int orderId)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/items").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
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

        public async Task<OrderMessage[]> GetOrderMessagesAsync(int orderId)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/messages").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var messages = ParseResponseArrayAllowEmpty<OrderMessage>(responseBody, 200, url, method);
            return messages;
        }

        public async Task<Feedback[]> GetOrderFeedbackAsync(int orderId)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/feedback").ToString();

            var method = HttpMethod.Get;
            var responseBody = await ExecuteRequest(url, method);
            var feedbackArray = ParseResponseArrayAllowEmpty<Feedback>(responseBody, 200, url, method);
            return feedbackArray;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/status").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, new
            {
                field = "status",
                value = status.GetStringValueOrDefault()
            });

            ParseResponseNoData(responseBody, 200, url, method);
            return;
        }

        public async Task UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}/payment_status").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, new
            {
                field = "payment_status",
                value = status.ToString()
            });

            ParseResponseNoData(responseBody, 200, url, HttpMethod.Put);
            return;
        }

        public async Task SendDriveThruAsync(int orderId, bool mailCcMe)
        {
            var builder = new UriBuilder(new Uri(_baseUri, $"orders/{orderId}/drive_thru"));
            var query = HttpUtility.ParseQueryString(builder.Query);
            query.Add("mail_me", mailCcMe.ToString());
            builder.Query = query.ToString();
            var url = builder.ToString();

            var method = HttpMethod.Post;
            var responseBody = await ExecuteRequest(url, method);

            ParseResponseNoData(responseBody, 200, url, method);
            return;
        }

        public async Task<OrderDetails> UpdateOrderAsync(int orderId, UpdateOrder updateOrder)
        {
            var url = new Uri(_baseUri, $"orders/{orderId}").ToString();

            var method = HttpMethod.Put;
            var responseBody = await ExecuteRequest(url, method, updateOrder, new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });

            var order = ParseResponse<OrderDetails>(responseBody, 200, url, method);
            return order;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
