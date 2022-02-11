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

using BricklinkSharp.Client.Enums;
using System;
using System.Text.Json.Serialization;

namespace BricklinkSharp.Client
{
    [Serializable]
    public class PartOutResult
    {
        [JsonPropertyName("average_6months_sales_value_usd")]
        public decimal Average6MonthsSalesValueUsd { get; }

        [JsonPropertyName("average_6months_sales_value_my_currency")]
        public decimal? Average6MonthsSalesValueMyCurrency { get; internal set; }

        [JsonPropertyName("included_items_count")]
        public int IncludedItemsCount { get; }

        [JsonPropertyName("included_lots_count")]
        public int IncludedLotsCount { get; }

        [JsonPropertyName("not_included_items_count")]
        public int NotIncludedItemsCount { get; }

        [JsonPropertyName("not_included_lots_count")]
        public int NotIncludeLotsCount { get; }

        [JsonPropertyName("current_sales_value_usd")]
        public decimal CurrentSalesValueUsd { get; }

        [JsonPropertyName("current_sales_value_my_currency")]
        public decimal? CurrentSalesValueMyCurreny { get; internal set; }

        [JsonPropertyName("item_number")]
        public string ItemNumber { get; internal set; } = string.Empty;

        [JsonPropertyName("condition")]
        public Condition Condition { get; internal set; }

        internal PartOutResult(decimal average6MonthsSalesValueUsd, decimal currentSalesValueUsd, 
            int includedItemsCount, int includedLotsCount, 
            int notIncludedItemsCount, int notIncludeLotsCount)
        {
            Average6MonthsSalesValueUsd = average6MonthsSalesValueUsd;
            CurrentSalesValueUsd = currentSalesValueUsd;
            IncludedItemsCount = includedItemsCount;
            IncludedLotsCount = includedLotsCount;
            NotIncludedItemsCount = notIncludedItemsCount;
            NotIncludeLotsCount = notIncludeLotsCount;
        }
    }
}