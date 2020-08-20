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

namespace BricklinkSharp.Client
{
    [Serializable]
    public class NewInventory : InventoryBase
    {
        [JsonPropertyName("item")]
        public ItemBase Item { get; set; }

        private int GetTieredPricePropertiesSetCount()
        {
            var count = 0;

            if (TierPrice1 > 0.0M)
            {
                count += 1;
            }

            if (TierPrice2 > 0.0M)
            {
                count += 1;
            }

            if (TierPrice3 > 0.0M)
            {
                count += 1;
            }

            if (TierQuantity1 > 0)
            {
                count += 1;
            }

            if (TierQuantity2 > 0)
            {
                count += 1;
            }

            if (TierQuantity3 > 0)
            {
                count += 1;
            }

            return count;
        }

        internal void ValidateThrowException()
        {
            if (Item.Type != ItemType.Set && Completeness != Completeness.Complete)
            {
                throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                    {
                        {"Item.Type", Item.Type},
                        {nameof(Completeness), Completeness}
                    },
                    $"{Completeness} can only be applied to items of type {ItemType.Set}.", 
                    GetType());
            }

            if (IsStockRoom && StockRoomId != "A" && StockRoomId != "B" && StockRoomId != "C")
            {
                throw new BricklinkInvalidParameterException(new Dictionary<string, object>
                    {
                        {nameof(StockRoomId), StockRoomId}
                    },
                    $"{nameof(StockRoomId)} must be either A, B or C (set to: {StockRoomId}.",
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

            if (Bulk < 1)
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
