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
public abstract class OrderBase
{
    [JsonPropertyName("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("date_ordered")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("seller_name")]
    public string SellerName { get; set; } = null!;

    [JsonPropertyName("buyer_name")]
    public string BuyerName { get; set; } = null!;

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("unique_count")]
    public int UniqueCount { get; set; }

    [JsonPropertyName("status"), JsonConverter(typeof(OrderStatusStringConverter))]
    public OrderStatus Status { get; set; }

    [JsonPropertyName("payment")]
    public Payment Payment { get; set; } = null!;
}