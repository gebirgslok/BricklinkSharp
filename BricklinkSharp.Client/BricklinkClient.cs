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
    internal class BricklinkClient : IBricklinkClient
    {
        private static readonly Uri _baseUri = new Uri("https://api.bricklink.com/api/store/v1/");

        private void GetAuthorizationHeader(string url, string method, out string scheme, out string parameter)
        {
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

        private async Task<string> ExecutePostRequest<TBody>(string url, TBody body)
        {
            var method = HttpMethod.Post;
            GetAuthorizationHeader(url, method.ToString(), out var authScheme, out var authParameter);

            using var client = new HttpClient();
            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.Default, "application/json");
            content.Headers.ContentType.CharSet = string.Empty;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);

            var response = await client.PostAsync(url, content);
            var contentAsString = await response.Content.ReadAsStringAsync();
            return contentAsString;
        }

        private async Task<string> ExecuteGetRequest(string url)
        {
            var method = HttpMethod.Get;
            GetAuthorizationHeader(url, method.ToString(), out var authScheme, out var authParameter);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);

            var response = await client.GetAsync(url);
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

        public async Task<CatalogItem> GetItemAsync(ItemType type, string no)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}").ToString();

            var responseBody = await ExecuteGetRequest(url);

            var data = ParseResponse<CatalogItem>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/images/{colorId}").ToString();

            var responseBody = await ExecuteGetRequest(url);

            var data = ParseResponse<CatalogImage>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Superset[]> GetSupersetsAsync(ItemType type, string no, int colorId)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/supersets/{colorId}").ToString();

            var responseBody = await ExecuteGetRequest(url);

            var data = ParseResponse<Superset[]>(responseBody, 200, url, HttpMethod.Get);
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
            var responseBody = await ExecuteGetRequest(builder.ToString());

            var data = ParseResponse<Subset[]>(responseBody, 200, url, HttpMethod.Get);
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
            var responseBody = await ExecuteGetRequest(builder.ToString());

            var data = ParseResponse<PriceGuide>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no)
        {
            var typeString = type.GetStringValueOrDefault();
            var url = new Uri(_baseUri, $"items/{typeString}/{no}/colors").ToString();

            var responseBody = await ExecuteGetRequest(url);

            var data = ParseResponse<KnownColor[]>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Color[]> GetColorListAsync()
        {
            var url = new Uri(_baseUri, "colors").ToString();
            var responseBody = await ExecuteGetRequest(url);
            var data = ParseResponse<Color[]>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Color> GetColorAsync(int colorId)
        {
            var url = new Uri(_baseUri, $"colors/{colorId}").ToString();
            var responseBody = await ExecuteGetRequest(url);
            var data = ParseResponse<Color>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Category[]> GetCategoryListAsync()
        {
            var url = new Uri(_baseUri, "categories").ToString();
            var responseBody = await ExecuteGetRequest(url);
            var data = ParseResponse<Category[]>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Category> GetCategoryAsync(int categoryId)
        {
            var url = new Uri(_baseUri, $"categories/{categoryId}").ToString();
            var responseBody = await ExecuteGetRequest(url);
            var data = ParseResponse<Category>(responseBody, 200, url, HttpMethod.Get);
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

            var responseBody = await ExecuteGetRequest(url);
            var dataArray = ParseResponseArrayAllowEmpty<Inventory>(responseBody, 200, url, HttpMethod.Get);
            return dataArray;
        }

        public async Task<Inventory> GetInventoryAsync(int inventoryId)
        {
            var url = new Uri(_baseUri, $"inventories/{inventoryId}").ToString();
            var responseBody = await ExecuteGetRequest(url);
            var data = ParseResponse<Inventory>(responseBody, 200, url, HttpMethod.Get);
            return data;
        }

        public async Task<Inventory> CreateInventoryAsync(NewInventory newInventory)
        {
            newInventory.ValidateThrowException();
            var url = new Uri(_baseUri, "inventories").ToString();
            var responseBody = await ExecutePostRequest(url, newInventory);

            var data = ParseResponse<Inventory>(responseBody, 201, url, HttpMethod.Post);
            return data;
        }

        public async Task CreateInventoriesAsync(NewInventory[] newInventories)
        {
            foreach (var newInventory in newInventories)
            {
                newInventory.ValidateThrowException();
            }

            var url = new Uri(_baseUri, "inventories").ToString();
            var responseBody = await ExecutePostRequest(url, newInventories);
            ParseResponseNoData(responseBody, 201, url, HttpMethod.Post);
        }
    }
}
