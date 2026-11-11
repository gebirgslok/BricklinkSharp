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

namespace BricklinkSharp.Client.CurrencyRates;

// Base for services that return a whole table of rates against one base currency:
// { "base": "EUR", "date": "...", "rates": { "USD": 1.08, ... } }
internal abstract class RatesTableFetcher : IFetchExchangeRate
{
    internal sealed class RatesTable
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();

        [JsonPropertyName("base")]
        public string Base { get; set; } = null!;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }

    internal sealed class CacheEntry
    {
        public RatesTable Table { get; }

        public DateTime FetchedAtUtc { get; }

        public CacheEntry(RatesTable table, DateTime fetchedAtUtc)
        {
            Table = table;
            FetchedAtUtc = fetchedAtUtc;
        }
    }

    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

    private readonly HttpClient _httpClient;

    protected RatesTableFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Each service keeps its own static cache, shared by all clients.
    protected abstract CacheEntry? Cache { get; set; }

    protected abstract string BuildUrl();

    private static decimal GetRate(string currencyUpper, RatesTable table)
    {
        if (string.Equals(currencyUpper, table.Base, StringComparison.OrdinalIgnoreCase))
        {
            return 1.0M;
        }

        if (!table.Rates.TryGetValue(currencyUpper, out var rate))
        {
            throw new CurrencyNotSupportedException(currencyUpper);
        }

        return rate;
    }

    public async Task<ExchangeRateResult> FetchAsync(string fromCurrency, string toCurrency,
        CancellationToken cancellationToken = default)
    {
        var cache = Cache;

        if (cache == null || DateTime.UtcNow - cache.FetchedAtUtc >= _cacheDuration)
        {
            var response = await _httpClient.GetAsync(BuildUrl(), cancellationToken);
            response.EnsureSuccessStatusCode();

#if HAVE_HTTP_CONTENT_READ_CANCELLATION_TOKEN
            var contentAsString = await response.Content.ReadAsStringAsync(cancellationToken);
#else
            var contentAsString = await response.Content.ReadAsStringAsync();
#endif
            var table = JsonSerializer.Deserialize<RatesTable>(contentAsString)!;
            cache = new CacheEntry(table, DateTime.UtcNow);
            Cache = cache;
        }

        var from = fromCurrency.ToUpperInvariant();
        var to = toCurrency.ToUpperInvariant();
        var rate = GetRate(to, cache.Table) / GetRate(from, cache.Table);

        return new ExchangeRateResult(from, to, rate, cache.Table.Date);
    }
}
