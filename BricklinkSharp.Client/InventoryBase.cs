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

namespace BricklinkSharp.Client
{
    [Serializable]
    public class InventoryBase
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("color_id")]
        public int ColorId { get; set; }

        [JsonPropertyName("unit_price"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("new_or_used"), JsonConverter(typeof(ConditionStringConverter))]
        public Condition Condition { get; set; }

        [JsonPropertyName("completeness"), JsonConverter(typeof(CompletenessStringConverter))]
        public Completeness Completeness { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("bulk")] 
        public int Bulk { get; set; } = 1;

        [JsonPropertyName("is_retain")]
        public bool IsRetain { get; set; }

        [JsonPropertyName("is_stock_room")]
        public bool IsStockRoom { get; set; }

        [JsonPropertyName("stock_room_id")]
        public string? StockRoomId { get; set; }

        [JsonPropertyName("my_cost"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal MyCost { get; set; }

        [JsonPropertyName("tier_quantity1")]
        public int TierQuantity1 { get; set; }

        [JsonPropertyName("tier_quantity2")]
        public int TierQuantity2 { get; set; }

        [JsonPropertyName("tier_quantity3")]
        public int TierQuantity3 { get; set; }

        [JsonPropertyName("tier_price1"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal TierPrice1 { get; set; }

        [JsonPropertyName("tier_price2"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal TierPrice2 { get; set; }

        [JsonPropertyName("tier_price3"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal TierPrice3 { get; set; }
    }
}