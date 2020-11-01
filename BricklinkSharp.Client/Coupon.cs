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

namespace BricklinkSharp.Client
{
    [Serializable]
    public class Coupon : CouponBase
    {
        [JsonPropertyName("coupon_id")]
        public int Id { get; set; }

        [JsonPropertyName("date_issued")]
        public DateTime DateIssued { get; set; }

        [JsonPropertyName("date_expire")]
        public DateTime DateExpire { get; set; }

        [JsonPropertyName("buyer_name")]
        public string BuyerName { get; set; } = null!;

        [JsonPropertyName("seller_name")]
        public string SellerName { get; set; } = null!;

        [JsonPropertyName("store_name")]
        public string StoreName { get; set; } = null!;

        [JsonPropertyName("status"), JsonConverter(typeof(CouponStatusStringConverter))]
        public CouponStatus Status { get; set; }

        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("disp_currency_code")]
        public string DisplayCurrencyCode { get; set; } = null!;

        [JsonPropertyName("disp_discount_amount"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal DisplayDiscountAmount { get; set; }

        [JsonPropertyName("disp_max_discount_amount"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal DisplayMaxDiscountAmount { get; set; }

        [JsonPropertyName("disp_tier_price1"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal DisplayTierPrice1 { get; set; }

        [JsonPropertyName("disp_tier_price2"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal DisplayTierPrice2 { get; set; }

        [JsonPropertyName("disp_tier_price3"), JsonConverter(typeof(DecimalStringConverter))]
        public decimal DisplayTierPrice3 { get; set; }               
    }
}
