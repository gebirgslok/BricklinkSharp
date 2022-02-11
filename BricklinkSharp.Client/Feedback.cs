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
using BricklinkSharp.Client.Enums;
using BricklinkSharp.Client.Json;

// ReSharper disable UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
namespace BricklinkSharp.Client
{
    public class Feedback : FeedbackBase
    {
        [JsonPropertyName("feedback_id")]
        public int FeedbackId { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; } = null!;

        [JsonPropertyName("to")]
        public string To { get; set; } = null!;

        [JsonPropertyName("date_rated")]
        public DateTime CreatedOn { get; set; }

        [JsonPropertyName("rating_of_bs"), JsonConverter(typeof(RatingTargetRoleStringConverter))]
        public RatingTargetRole RatingTargetRole { get; set; }

        [JsonPropertyName("reply")]
        public string? Reply { get; set; }

    }
}
