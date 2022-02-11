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
using System.Text.Json;
using System.Text.Json.Serialization;
using BricklinkSharp.Client.Enums;
using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client.Json
{
    internal class ItemTypeStringConverter : JsonConverter<ItemType>
    {
        public override ItemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var stringValue = reader.GetString();

            if (stringValue == null)
            {
                throw new NullReferenceException(nameof(stringValue));
            }

            var removedUnderscore = stringValue.Replace("_", string.Empty);

            if (Enum.TryParse(removedUnderscore, true, out ItemType result))
            {
                return result;
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, ItemType value, JsonSerializerOptions options)
        {
            var typeString = value.ToDomainString().ToUpperInvariant();
            writer.WriteStringValue(typeString);
        }
    }
}
