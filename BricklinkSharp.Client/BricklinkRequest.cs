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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BricklinkSharp.Client;

internal sealed class BricklinkRequest
{
    private KeyValuePair<string, string>[] _queryParameters;

    public BricklinkRequest(BricklinkApiResourceType resource, HttpVerb verb, string path)
    {
        Resource = resource;
        Verb = verb;
        Path = path;
        _queryParameters = Array.Empty<KeyValuePair<string, string>>();
    }

    private BricklinkRequest(BricklinkRequest other)
    {
        Resource = other.Resource;
        Verb = other.Verb;
        Path = other.Path;
        _queryParameters = other._queryParameters;
        Body = other.Body;
        Credentials = other.Credentials;
        ExpectedMetaCode = other.ExpectedMetaCode;
        Shape = other.Shape;
    }

    public BricklinkApiResourceType Resource { get; }

    public HttpVerb Verb { get; }

    public string Path { get; }

    public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => _queryParameters;

    public object? Body { get; private set; }

    public BricklinkCredentials? Credentials { get; private set; }

    public int? ExpectedMetaCode { get; private set; }

    public ResponseShape? Shape { get; private set; }

    public BricklinkRequest WithQueryParameter(string name, object? value)
    {
        if (value is null)
        {
            return this;
        }

        var copy = new BricklinkRequest(this);
        copy._queryParameters = _queryParameters.Append(new KeyValuePair<string, string>(name, FormatQueryValue(value))).ToArray();
        return copy;
    }

    public BricklinkRequest WithBody(object? body)
    {
        return new BricklinkRequest(this) { Body = body };
    }

    public BricklinkRequest WithCredentials(BricklinkCredentials? credentials)
    {
        return new BricklinkRequest(this) { Credentials = credentials };
    }

    public BricklinkRequest WithExpectedMetaCode(int metaCode)
    {
        return new BricklinkRequest(this) { ExpectedMetaCode = metaCode };
    }

    public BricklinkRequest WithShape(ResponseShape shape)
    {
        return new BricklinkRequest(this) { Shape = shape };
    }

    public string BuildUrl(Uri baseUri)
    {
        var builder = new StringBuilder();
        builder.Append(baseUri.AbsoluteUri.TrimEnd('/'));
        builder.Append('/');
        builder.Append(Path.TrimStart('/'));

        for (var i = 0; i < _queryParameters.Length; i++)
        {
            builder.Append(i == 0 ? '?' : '&');
            builder.Append(Uri.EscapeDataString(_queryParameters[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(_queryParameters[i].Value));
        }

        return builder.ToString();
    }

    private static string FormatQueryValue(object value)
    {
        return value switch
        {
            string text => text,
            bool flag => flag ? "true" : "false",
            Enum => throw new ArgumentException(
                "Enum query values must be passed as their BrickLink domain string.", nameof(value)),
            DateTime date => date.ToString("o", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString("o", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
