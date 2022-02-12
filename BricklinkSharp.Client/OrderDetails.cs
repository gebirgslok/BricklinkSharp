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
public class OrderDetails : OrderBase
{
    [JsonPropertyName("store_name")]
    public string? StoreName { get; set; }

    [JsonPropertyName("buyer_email")]
    public string BuyerEmail { get; set; } = null!;

    [JsonPropertyName("buyer_order_count")]
    public int BuyerOrderCount { get; set; }

    [JsonPropertyName("require_insurance")]
    public bool RequireInsurance { get; set; }

    [JsonPropertyName("is_invoiced")]
    public bool IsInvoiced { get; set; }

    [JsonPropertyName("is_filed")]
    public bool IsFiled { get; set; }

    [JsonPropertyName("drive_thru_sent")]
    public bool WasDriveThruSent { get; set; }

    [JsonPropertyName("salesTax_collected_by_bl")]
    public bool WasSalesTaxCollectedByBricklink { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("total_weight"), JsonConverter(typeof(DoubleStringConverter))]
    public double TotalWeight { get; set; }

    [JsonPropertyName("shipping")]
    public Shipping Shipping { get; set; } = null!;

    [JsonPropertyName("cost")]
    public Cost Cost { get; set; } = null!;

    [JsonPropertyName("disp_cost")]
    public Cost DisplayCost { get; set; } = null!;
}