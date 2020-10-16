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

namespace BricklinkSharp.Client
{
    [Serializable]
    public class UpdateCost
    {
        [JsonPropertyName("credit")]
        public decimal? Credit { get; set; }

        [JsonPropertyName("insurance")]
        public decimal? Insurance { get; set; }

        [JsonPropertyName("etc1")]
        public decimal? Etc1 { get; set; }

        [JsonPropertyName("etc2")]
        public decimal? Etc2 { get; set; }

        [JsonPropertyName("shipping")]
        public decimal? Shipping { get; set; }

        internal bool AllPropertiesUnset => Credit != null &&
                                  Insurance != null &&
                                  Shipping != null &&
                                  Etc1 != null &&
                                  Etc2 != null;

        internal UpdateCost()
        {
        }
    }
}