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

using BricklinkSharp.Client.Json;
using System;
using System.Text.Json.Serialization;

namespace BricklinkSharp.Client;
//[Serializable]
//public class ItemsBatchList
//{
//    public IReadOnlyList<OrderItem[]>
//}

[Serializable]
public class OrderItem
{
    [JsonPropertyName("inventory_id")]
    public int InventoryId { get; set; }

    [JsonPropertyName("item")]
    public InventoryItem Item { get; set; } = null!;

    [JsonPropertyName("color_id")]
    public int ColorId { get; set; }

    [JsonPropertyName("color_name")]
    public string? ColorName { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("new_or_used"), JsonConverter(typeof(ConditionStringConverter))]
    public Condition Condition { get; set; }

    [JsonPropertyName("completeness"), JsonConverter(typeof(CompletenessStringConverter))]
    public Completeness Completeness { get; set; }

    [JsonPropertyName("unit_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("unit_price_final"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal UnitPriceFinal { get; set; }

    [JsonPropertyName("disp_unit_price"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal DisplayUnitPrice { get; set; }

    [JsonPropertyName("disp_unit_price_final"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal DisplayUnitPriceFinal { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = null!;

    [JsonPropertyName("disp_currency_code")]
    public string DisplayCurrencyCode { get; set; } = null!;

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("weight"), JsonConverter(typeof(DoubleStringConverter))]
    public double Weight { get; set; }
}