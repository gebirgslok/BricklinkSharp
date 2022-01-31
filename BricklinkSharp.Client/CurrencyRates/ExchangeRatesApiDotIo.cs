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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BricklinkSharp.Client.CurrencyRates
{
    // Calls https://exchangeratesapi.io/ Foreign exchange rate service
    internal class ExchangeRatesApiDotIo : IExchangeRatesService
    {
        class ExchangeRatesApiResponse
        {
            [JsonPropertyName("rates")]
            public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();

            [JsonPropertyName("base")]
            public string Base { get; set; } = null!;

            [JsonPropertyName("date")]
            public DateTime Date { get; set; }
        }

        private readonly HttpClient _httpClient;

        private static ExchangeRatesApiResponse? _cachedResponse;

        public ExchangeRatesApiDotIo(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static decimal GetRate(string currency, ExchangeRatesApiResponse response)
        {
            var currencyUpper = currency.ToUpperInvariant();

            if (currencyUpper == "EUR")
            {
                return 1.0M;
            }

            if (!response.Rates.ContainsKey(currencyUpper))
            {
                throw new CurrencyNotSupportedException(currencyUpper);
            }

            return response.Rates[currencyUpper];
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency, 
            CancellationToken cancellationToken = default)
        {
            if (_cachedResponse == null || (DateTime.Now - _cachedResponse.Date) >= TimeSpan.FromHours(24))
            {
                var key = ExchangeRatesApiDotIoConfiguration.Instance.ApiKey;

                if (key == null)
                {
                    throw new BricklinkMissingCredentialsException(new List<string>{ "Exchangeratesapi.io - API Access Key" });
                }

                var response = await _httpClient.GetAsync($"http://api.exchangeratesapi.io/v1/latest?access_key={key}", 
                    cancellationToken);
                response.EnsureSuccessStatusCode();

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
                var contentAsString = await response.Content.ReadAsStringAsync(cancellationToken);
#else
                var contentAsString = await response.Content.ReadAsStringAsync();
#endif
                _cachedResponse = JsonSerializer.Deserialize<ExchangeRatesApiResponse>(contentAsString);
            }

            return GetRate(toCurrency, _cachedResponse!) / GetRate(fromCurrency, _cachedResponse!);
        }
    }
}
