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

using System.Net.Http;

namespace BricklinkSharp.Client.CurrencyRates;

/// <summary>
/// Ready-made <see cref="IFetchExchangeRate"/> clients.
/// </summary>
public static class ExchangeRateServices
{
    /// <summary>
    /// https://exchangeratesapi.io/ (the default). Requires <see cref="ExchangeRatesApiDotIoConfiguration.ApiKey"/>.
    /// </summary>
    public static IFetchExchangeRate CreateExchangeRatesApiDotIo(HttpClient httpClient)
    {
        return new ExchangeRatesApiDotIo(httpClient);
    }

    /// <summary>
    /// https://allratestoday.com/ ECB reference rates, no API key needed. Where the rates are shown publicly,
    /// a visible "Exchange rates by AllRatesToday" link to https://allratestoday.com is required.
    /// </summary>
    public static IFetchExchangeRate CreateAllRatesToday(HttpClient httpClient)
    {
        return new AllRatesToday(httpClient);
    }
}
