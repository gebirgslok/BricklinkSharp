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

public class BricklinkHttpErrorException : BricklinkException
{
    public int ExpectedStatusCode { get; }

    public int ReceivedStatusCode { get; }

    public string? Description { get; }

    public string? RawMessage { get; }

    internal BricklinkHttpErrorException(int receivedStatusCode, int expectedStatusCode, string description, string message, string url, HttpMethod httpMethod)
        : base($"Received unexpected HTTP status code for request '{httpMethod}' {url}:" +
               Environment.NewLine +
               $"Expected = {expectedStatusCode}, received = {receivedStatusCode}." +
               Environment.NewLine +
               $"Description: {description}, message: {message}.", url, httpMethod)
    {
        ExpectedStatusCode = expectedStatusCode;
        ReceivedStatusCode = receivedStatusCode;
        Description = description;
        RawMessage = message;
    }
}