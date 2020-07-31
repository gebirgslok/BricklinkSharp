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
using System.Net.Http;
using System.Net.Http.Headers;
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

        private async Task<string> ExecuteGetRequest(string url)
        {
            var method = HttpMethod.Get;
            GetAuthorizationHeader(url, method.ToString(), out var authScheme, out var authParameter);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authScheme, authParameter);

            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        private TData ParseResponse<TData>(string responseBody, int expectedCode, string url, HttpMethod httpMethod)
        {
            using var json = JsonDocument.Parse(responseBody);
            var meta = json.RootElement.GetProperty("meta").ToObject<ResponseMeta>();

            if (meta.Code != expectedCode)
            {
                throw new BricklinkHttpErrorException(meta.Code, expectedCode, meta.Description, meta.Message, url, httpMethod);
            }

            var dataElement = json.RootElement.GetProperty("data");

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

        public Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType> includedItemTypes = null, 
            IEnumerable<ItemType> excludedItemTypes = null,
            IEnumerable<InventoryStatusType> includedStatusFlags = null, 
            IEnumerable<InventoryStatusType> excludedStatusFlags = null,
            IEnumerable<int> includedCategoryIds = null, 
            IEnumerable<int> excludedCategoryIds = null, 
            IEnumerable<int> includedColorIds = null,
            IEnumerable<int> excludedColorIds = null)
        {
            throw new NotImplementedException();
        }
    }
}
