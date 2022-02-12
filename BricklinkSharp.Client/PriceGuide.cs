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
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Json;

namespace BricklinkSharp.Client;

[Serializable]
public class PriceGuide
{
    [JsonPropertyName("item")]
    public ItemBase Item { get; set; } = null!;

    [JsonPropertyName("new_or_used"), JsonConverter(typeof(ConditionStringConverter))]
    public Condition Condition { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = null!;

    [JsonPropertyName("min_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal MinPrice { get; set; }

    [JsonPropertyName("max_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal MaxPrice { get; set; }

    [JsonPropertyName("avg_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal AveragePrice { get; set; }

    [JsonPropertyName("qty_avg_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal QuantityAveragePrice { get; set; }

    [JsonPropertyName("unit_quantity")]
    public int UnitQuantity { get; set; }

    [JsonPropertyName("total_quantity")]
    public int TotalQuantity { get; set; }

    [JsonPropertyName("price_detail")]
    public PriceDetail[]? PriceDetails { get; set; }

    public override string ToString()
    {
        return $"({Item.Type}, {Item.Number}): {Condition}, {CurrencyCode}, min: {MinPrice}, " + 
               $"max: {MaxPrice}, avg: {AveragePrice}, qty_avg: {QuantityAveragePrice}";
    }
}