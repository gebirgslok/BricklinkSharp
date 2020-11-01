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
    public class ShippingMethod
    {
        [JsonPropertyName("method_id")]
        public int MethodId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("insurance")]
        public bool HasInsurance { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }

        [JsonPropertyName("area"), JsonConverter(typeof(ShippingAreaStringConverter))]
        public ShippingArea Area { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        public override string ToString()
        {
            return $"{nameof(MethodId)}: {MethodId}, {nameof(ShippingArea)}: {Area}, {nameof(Name)}: {Name}" +
                   Environment.NewLine +
                   $"{nameof(Note)}: {Note ?? "null"}" +
                   Environment.NewLine +
                   $"{nameof(HasInsurance)}: {HasInsurance}, {nameof(IsDefault)}: {IsDefault}, {nameof(IsAvailable)}: {IsAvailable}";
        }
    }
}
