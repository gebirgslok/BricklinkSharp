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
using System.Linq;
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Json;
using NullGuard;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BricklinkSharp.Client
{
    public class UpdatedInventory
    {
        [JsonPropertyName("quantity"), JsonConverter(typeof(ChangedQuantityStringConverter))]
        public int? ChangedQuantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal? UnitPrice { get; set; }

        [JsonPropertyName("description"), AllowNull]
        public string Description { get; set; }

        [JsonPropertyName("remarks"), AllowNull]
        public string Remarks { get; set; }

        [JsonPropertyName("bulk")]
        public int? Bulk { get; set; }

        [JsonPropertyName("is_retain")]
        public bool? IsRetain { get; set; }

        [JsonPropertyName("is_stock_room")]
        public bool? IsStockRoom { get; set; }

        [JsonPropertyName("stock_room_id "), AllowNull]
        public string StockRoomId { get; set; }

        [JsonPropertyName("my_cost")]
        public decimal? MyCost { get; set; }

        [JsonPropertyName("sale_rate")]
        public int? SaleRate { get; set; }

        [JsonPropertyName("tier_quantity1")]
        public int? TierQuantity1 { get; set; }

        [JsonPropertyName("tier_quantity2")]
        public int? TierQuantity2 { get; set; }

        [JsonPropertyName("tier_quantity3")]
        public int? TierQuantity3 { get; set; }

        [JsonPropertyName("tier_price1")]
        public decimal? TierPrice1 { get; set; }

        [JsonPropertyName("tier_price2")]
        public decimal? TierPrice2 { get; set; }

        [JsonPropertyName("tier_price3")]
        public decimal? TierPrice3 { get; set; }

        private int GetTieredPricePropertiesSetCount()
        {
            var count = 0;

            if (TierPrice1 != null)
            {
                count += 1;
            }

            if (TierPrice2 != null)
            {
                count += 1;
            }

            if (TierPrice3 != null)
            {
                count += 1;
            }

            if (TierQuantity1 != null)
            {
                count += 1;
            }

            if (TierQuantity2 != null)
            {
                count += 1;
            }

            if (TierQuantity3 != null)
            {
                count += 1;
            }

            return count;
        }

        internal void ValidateThrowException()
        {
            if (IsStockRoom.HasValue && IsStockRoom.Value && StockRoomId != "A" && StockRoomId != "B" && StockRoomId != "C")
            {
                throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                    {
                        {nameof(StockRoomId), StockRoomId ?? "null"}
                    },
                    $"{nameof(StockRoomId)} must be either A, B or C (set to: {StockRoomId ?? "null"}.",
                    GetType());
            }

            var setCount = GetTieredPricePropertiesSetCount();
            if (setCount > 0 && setCount < 6)
            {
                throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                    {
                        {nameof(TierQuantity1), TierQuantity1},
                        {nameof(TierQuantity2), TierQuantity2},
                        {nameof(TierQuantity3), TierQuantity3},
                        {nameof(TierPrice1), TierPrice1},
                        {nameof(TierPrice2), TierPrice2},
                        {nameof(TierPrice3), TierPrice3},
                    },
                    $"All properties ({nameof(TierPrice1)}-{nameof(TierPrice3)}, {nameof(TierQuantity1)}-{nameof(TierQuantity3)}) must be set" +
                    "when using tiered pricing.",
                    GetType());
            }

            if (setCount == 6)
            {
                var invalidParameters = new Dictionary<string, object>();
                if (TierQuantity2 >= TierQuantity1)
                {
                    invalidParameters.Add(nameof(TierQuantity2), TierQuantity2);
                }

                if (TierQuantity3 >= TierQuantity2)
                {
                    invalidParameters.Add(nameof(TierQuantity3), TierQuantity3);
                }

                if (TierPrice1 >= UnitPrice)
                {
                    invalidParameters.Add(nameof(TierPrice1), TierPrice1);
                }

                if (TierPrice2 >= TierPrice1)
                {
                    invalidParameters.Add(nameof(TierPrice2), TierPrice2);
                }

                if (TierPrice3 >= TierPrice2)
                {
                    invalidParameters.Add(nameof(TierPrice3), TierPrice3);
                }

                if (invalidParameters.Any())
                {
                    throw new BricklinkInvalidParameterException(invalidParameters,
                        "Following rules apply for tiered pricing: " + Environment.NewLine +
                        $"{nameof(UnitPrice)} > {nameof(TierPrice1)} > {nameof(TierPrice2)} > {nameof(TierPrice3)}," + Environment.NewLine +
                        $"{nameof(TierQuantity1)} < {nameof(TierQuantity2)} < {nameof(TierQuantity3)}.",
                        GetType());
                }
            }

            if (Bulk.HasValue && Bulk.Value < 1)
            {
                throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                    {
                        {nameof(Bulk), Bulk}
                    },
                    $"{nameof(Bulk)} must be > 0.",
                    GetType());
            }
        }
    }
}
