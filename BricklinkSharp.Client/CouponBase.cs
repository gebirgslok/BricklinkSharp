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
public abstract class CouponBase
{
    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("applies_to")]
    public CouponAppliesTo AppliesTo { get; set; } = null!;

    [JsonPropertyName("discount_type"), JsonConverter(typeof(DiscountTypeStringConverter))]
    public DiscountType DiscountType { get; set; }

    [JsonPropertyName("discount_amount"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal DiscountAmount { get; set; }

    [JsonPropertyName("discount_rate")]
    public int DiscountRate { get; set; }

    [JsonPropertyName("max_discount_amount"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal MaxDiscountAmount { get; set; }

    [JsonPropertyName("tier_price1"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal TierPrice1 { get; set; }

    [JsonPropertyName("tier_discount_rate1")]
    public int TierDiscountRate1 { get; set; }

    [JsonPropertyName("tier_price2"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal TierPrice2 { get; set; }

    [JsonPropertyName("tier_discount_rate2")]
    public int TierDiscountRate2 { get; set; }

    [JsonPropertyName("tier_price3"), JsonConverter(typeof(DecimalStringConverter))]
    public decimal TierPrice3 { get; set; }

    [JsonPropertyName("tier_discount_rate3")]
    public int TierDiscountRate3 { get; set; }
}
