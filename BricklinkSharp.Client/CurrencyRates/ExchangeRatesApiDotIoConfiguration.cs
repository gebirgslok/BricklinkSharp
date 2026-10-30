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

namespace BricklinkSharp.Client.CurrencyRates;

public class ExchangeRatesApiDotIoConfiguration
{
    private static ExchangeRatesApiDotIoConfiguration? _instance;

    public static ExchangeRatesApiDotIoConfiguration Instance => _instance ??= new ExchangeRatesApiDotIoConfiguration();

    public string? ApiKey { get; set;  }

    /// <summary>
    /// Default endpoint of the exchangeratesapi.io service.
    /// </summary>
    public const string DefaultBaseUrl = "http://api.exchangeratesapi.io/v1";

    /// <summary>
    /// Base URL of an exchangeratesapi.io-compatible endpoint (the client calls <c>{BaseUrl}/latest</c>).
    /// Defaults to exchangeratesapi.io, which requires <see cref="ApiKey"/>. When a different endpoint is set,
    /// the API key is optional and is only sent if provided.
    /// </summary>
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    private ExchangeRatesApiDotIoConfiguration()
    {
    }
}