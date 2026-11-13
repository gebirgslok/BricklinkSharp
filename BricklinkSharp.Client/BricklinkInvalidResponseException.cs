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
using System.Net.Http;

namespace BricklinkSharp.Client;

public class BricklinkInvalidResponseException : BricklinkException
{
    private const int BodyExcerptLength = 200;

    public int HttpStatusCode { get; }

    internal BricklinkInvalidResponseException(string reason, int httpStatusCode, string responseBody,
        string url, HttpMethod httpMethod, Exception? innerException = null)
        : base($"Received an invalid response for request '{httpMethod}' {url} (HTTP status {httpStatusCode}): {reason}" +
               Environment.NewLine +
               $"Body: {Excerpt(responseBody)}", url, httpMethod, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }

    private static string Excerpt(string responseBody)
    {
        return responseBody.Length <= BodyExcerptLength
            ? responseBody
            : responseBody.Substring(0, BodyExcerptLength) + "...";
    }
}
