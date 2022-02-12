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

using System.Text.Json.Serialization;
using BricklinkSharp.Client.Json;

namespace BricklinkSharp.Client;

public class Cost : CostBase
{
    [JsonPropertyName("salesTax_collected_by_BL"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal SalesTaxCollectedByBricklink { get; set; }

    [JsonPropertyName("final_total"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal FinalTotal { get; set; }

    [JsonPropertyName("etc1"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Etc1 { get; set; }

    [JsonPropertyName("etc2"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Etc2 { get; set; }

    [JsonPropertyName("insurance"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Insurance { get; set; }

    [JsonPropertyName("shipping"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Shipping { get; set; }

    [JsonPropertyName("credit"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Credit { get; set; }

    [JsonPropertyName("coupon"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal Coupon { get; set; }

    [JsonPropertyName("vat_rate"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal VatRate { get; set; }

    [JsonPropertyName("vat_amount"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal VatAmount { get; set; }
}