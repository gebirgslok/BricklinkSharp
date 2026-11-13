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
using System.Collections;
using System.Net.Http;
using System.Text.Json;

namespace BricklinkSharp.Client;

internal static class BricklinkResponseParser
{
    internal static ResponseShape ResolveShape<T>(ResponseShape? requested)
    {
        if (requested.HasValue)
        {
            return requested.Value;
        }

        if (typeof(T) == typeof(NoData))
        {
            return ResponseShape.NoData;
        }

        var isCollection = typeof(T) != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeof(T));
        return isCollection ? ResponseShape.ArrayOrEmpty : ResponseShape.Object;
    }

    internal static T Parse<T>(string responseBody, int httpStatusCode, int expectedMetaCode, ResponseShape shape,
        string url, HttpMethod httpMethod)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(responseBody);
        }
        catch (JsonException exception)
        {
            throw new BricklinkInvalidResponseException("the body is not valid JSON", httpStatusCode,
                responseBody, url, httpMethod, exception);
        }

        using (document)
        {
            var root = document.RootElement;
            var meta = ReadMeta(root, httpStatusCode, responseBody, url, httpMethod);

            if (meta.Code != expectedMetaCode)
            {
                throw new BricklinkHttpErrorException(meta.Code, expectedMetaCode, meta.Description, meta.Message,
                    url, httpMethod);
            }

            root.TryGetProperty("data", out var data);
            return ReadData<T>(data, shape, httpStatusCode, responseBody, url, httpMethod);
        }
    }

    private static ResponseMeta ReadMeta(JsonElement root, int httpStatusCode, string responseBody,
        string url, HttpMethod httpMethod)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("meta", out var metaElement)
            || metaElement.ValueKind != JsonValueKind.Object)
        {
            throw new BricklinkInvalidResponseException("the 'meta' object is missing", httpStatusCode,
                responseBody, url, httpMethod);
        }

        try
        {
            return JsonSerializer.Deserialize<ResponseMeta>(metaElement.GetRawText())!;
        }
        catch (JsonException exception)
        {
            throw new BricklinkInvalidResponseException("the 'meta' object is malformed", httpStatusCode,
                responseBody, url, httpMethod, exception);
        }
    }

    private static T ReadData<T>(JsonElement data, ResponseShape shape, int httpStatusCode, string responseBody,
        string url, HttpMethod httpMethod)
    {
        var isAbsent = data.ValueKind == JsonValueKind.Undefined || data.ValueKind == JsonValueKind.Null;

        switch (shape)
        {
            case ResponseShape.NoData:
                if (typeof(T) != typeof(NoData))
                {
                    throw new InvalidOperationException(
                        $"{nameof(ResponseShape.NoData)} requires {nameof(NoData)} as the result type, not {typeof(T)}.");
                }

                EnsureNoData(data, isAbsent, url, httpMethod);
                return default!;
            case ResponseShape.Object:
                if (isAbsent || IsEmptyObject(data))
                {
                    throw new BricklinkNoDataReceivedException(url, httpMethod);
                }

                EnsureKind(data, JsonValueKind.Object, url, httpMethod);
                break;
            default:
                if (isAbsent)
                {
                    throw new BricklinkNoDataReceivedException(url, httpMethod);
                }

                EnsureKind(data, JsonValueKind.Array, url, httpMethod);

                if (shape == ResponseShape.ArrayNonEmpty && data.GetArrayLength() == 0)
                {
                    throw new BricklinkNoDataReceivedException(url, httpMethod);
                }

                break;
        }

        return Deserialize<T>(data, httpStatusCode, responseBody, url, httpMethod);
    }

    private static void EnsureNoData(JsonElement data, bool isAbsent, string url, HttpMethod httpMethod)
    {
        if (isAbsent || IsEmptyObject(data))
        {
            return;
        }

        EnsureKind(data, JsonValueKind.Object, url, httpMethod);
        throw new BricklinkEmptyDataExpectedException(data, url, httpMethod);
    }

    private static void EnsureKind(JsonElement data, JsonValueKind expected, string url, HttpMethod httpMethod)
    {
        if (data.ValueKind != expected)
        {
            throw new BricklinkUnexpectedDataKindException(expected.ToString(), data.ValueKind.ToString(),
                url, httpMethod);
        }
    }

    private static bool IsEmptyObject(JsonElement data)
    {
        return data.ValueKind == JsonValueKind.Object && !data.EnumerateObject().MoveNext();
    }

    private static T Deserialize<T>(JsonElement data, int httpStatusCode, string responseBody, string url,
        HttpMethod httpMethod)
    {
        T? result;

        try
        {
            result = JsonSerializer.Deserialize<T>(data.GetRawText());
        }
        catch (Exception exception) when (exception is JsonException
                                              or ArgumentException
                                              or FormatException
                                              or InvalidOperationException
                                              or NotSupportedException
                                              or NullReferenceException
                                              or OverflowException)
        {
            // custom converters throw these for unknown or malformed values
            throw new BricklinkInvalidResponseException("the 'data' element could not be mapped", httpStatusCode,
                responseBody, url, httpMethod, exception);
        }

        return result ?? throw new BricklinkNoDataReceivedException(url, httpMethod);
    }
}
