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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BricklinkSharp.Client.Tests.Support;

/// <summary>
/// Runs a single converter in isolation, with the wire value given as the raw JSON token.
/// </summary>
internal static class ConverterHarness
{
    internal static T? Read<T>(JsonConverter<T> converter, string jsonToken) =>
        JsonSerializer.Deserialize<T>(jsonToken, OptionsWith(converter));

    internal static string Write<T>(JsonConverter<T> converter, T value) =>
        JsonSerializer.Serialize(value, OptionsWith(converter));

    internal static string Quote(string wire) => "\"" + wire + "\"";

    private static JsonSerializerOptions OptionsWith(JsonConverter converter)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        return options;
    }
}
