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

// ReSharper disable ClassNeverInstantiated.Global
#nullable enable
#pragma warning disable 8618
namespace BricklinkSharp.Client
{
    [Serializable]
    public class CatalogItem : ItemBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("alternate_no")]
        public string? AlternateNo { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string? ThumbnailUrl { get; set; }

        [JsonPropertyName("weight"), JsonConverter(typeof(DoubleStringConverter))]
        public double Weight { get; set; }

        [JsonPropertyName("dim_x"), JsonConverter(typeof(DoubleStringConverter))]
        public double DimX { get; set; }

        [JsonPropertyName("dim_y"), JsonConverter(typeof(DoubleStringConverter))]
        public double DimY { get; set; }

        [JsonPropertyName("dim_z"), JsonConverter(typeof(DoubleStringConverter))]
        public double DimZ { get; set; }

        [JsonPropertyName("year_released")]
        public int YearReleased { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("is_obsolete")]
        public bool IsObsolete { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}: {Name}";
        }
    }
}
