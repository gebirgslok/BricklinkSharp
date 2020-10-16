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
using NullGuard;

namespace BricklinkSharp.Client
{
    [Serializable]
    public class Shipping
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("method_id")]
        public int Id { get; set; }

        [JsonPropertyName("tracking_no"), AllowNull]
        public string TrackingNo { get; set; }

        [JsonPropertyName("tracking_link")]
        public string TrackingLink { get; set; }

        [JsonPropertyName("date_shipped")]
        public DateTime ShipmentDate { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }
    }
}
