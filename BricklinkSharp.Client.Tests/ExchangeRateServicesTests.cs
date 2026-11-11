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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BricklinkSharp.Client.CurrencyRates;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class ExchangeRateServicesTests
{
    private const string Table = "{\"base\":\"EUR\",\"date\":\"2026-11-10\",\"rates\":{\"USD\":1.25,\"GBP\":0.85}}";

    private sealed class StubHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Table, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FixedRate : IFetchExchangeRate
    {
        public Task<ExchangeRateResult> FetchAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ExchangeRateResult(fromCurrency, toCurrency, 2M, DateTime.Today));
        }
    }

    [SetUp]
    public void ResetCaches()
    {
        AllRatesToday.ResetCache();
        ExchangeRatesApiDotIo.ResetCache();
        ExchangeRatesApiDotIoConfiguration.Instance.ApiKey = null;
    }

    [Test]
    public async Task AllRatesToday_ComputesCrossRate_WithoutKey()
    {
        var handler = new StubHandler();
        var service = ExchangeRateServices.CreateAllRatesToday(new HttpClient(handler));

        var result = await service.FetchAsync("usd", "GBP");

        ClassicAssert.AreEqual(0.68M, result.Rate);
        ClassicAssert.AreEqual("USD", result.FromCurrency);
        ClassicAssert.AreEqual(new DateTime(2026, 11, 10), result.Date);
        ClassicAssert.AreEqual(AllRatesToday.LatestUrl, handler.LastUri!.ToString());
    }

    [Test]
    public async Task AllRatesToday_BaseCurrencyIsOne_AndTableIsCached()
    {
        var handler = new StubHandler();
        var service = ExchangeRateServices.CreateAllRatesToday(new HttpClient(handler));

        var toEur = await service.FetchAsync("USD", "EUR");
        await service.FetchAsync("EUR", "GBP");

        ClassicAssert.AreEqual(0.8M, toEur.Rate);
        ClassicAssert.AreEqual(1, handler.Calls);
    }

    [Test]
    public void AllRatesToday_UnknownCurrency_Throws()
    {
        var service = ExchangeRateServices.CreateAllRatesToday(new HttpClient(new StubHandler()));

        Assert.ThrowsAsync<CurrencyNotSupportedException>(() => service.FetchAsync("USD", "XYZ"));
    }

    [Test]
    public void ExchangeRatesApiDotIo_WithoutKey_ThrowsMissingCredentials()
    {
        var handler = new StubHandler();
        var service = ExchangeRateServices.CreateExchangeRatesApiDotIo(new HttpClient(handler));

        Assert.ThrowsAsync<BricklinkMissingCredentialsException>(() => service.FetchAsync("USD", "EUR"));
        ClassicAssert.AreEqual(0, handler.Calls);
    }

    [Test]
    public async Task ExchangeRatesApiDotIo_WithKey_SendsAccessKey()
    {
        ExchangeRatesApiDotIoConfiguration.Instance.ApiKey = "abc";
        var handler = new StubHandler();
        var service = ExchangeRateServices.CreateExchangeRatesApiDotIo(new HttpClient(handler));

        await service.FetchAsync("USD", "EUR");

        StringAssert.EndsWith("/latest?access_key=abc", handler.LastUri!.ToString());
    }

    [Test]
    public void Factory_AcceptsCustomService()
    {
        using var httpClient = new HttpClient(new StubHandler());

        using var client = BricklinkClientFactory.Build(httpClient, null, new FixedRate());
        using var client2 = BricklinkClientFactory.Build(null, new FixedRate());

        ClassicAssert.NotNull(client);
        ClassicAssert.NotNull(client2);
    }
}
