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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace BricklinkSharp.Client;

/// <summary>
/// What to send and how to read the response for one API operation. Pure data: building one does no I/O.
/// A <see cref="Rejection"/> means the arguments were invalid and nothing may be sent.
/// </summary>
internal sealed class Endpoint<T>
{
    private readonly Func<Uri, HttpMethod, int, string, T>? _responseReader;

    internal Endpoint(ApiResourceType resource, HttpVerb verb, string path, QueryString query, object? body,
        int expectedMetaCode, ResponseShape shape, Func<Uri, HttpMethod, int, string, T>? responseReader,
        Exception? rejection)
    {
        Resource = resource;
        Verb = verb;
        Path = path;
        Query = query;
        Body = body;
        ExpectedMetaCode = expectedMetaCode;
        Shape = shape;
        _responseReader = responseReader;
        Rejection = rejection;
    }

    public ApiResourceType Resource { get; }

    public HttpVerb Verb { get; }

    public string Path { get; }

    public QueryString Query { get; }

    public object? Body { get; }

    public int ExpectedMetaCode { get; }

    public ResponseShape Shape { get; }

    public Exception? Rejection { get; }

    /// <summary>Set only by <see cref="Map{TResult}"/>; null means the default response policy reads <typeparamref name="T"/> directly.</summary>
    internal Func<Uri, HttpMethod, int, string, T>? ResponseReader => _responseReader;

    public static Endpoint<T> Get(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Get, path, shape, expectedMetaCode);

    public static Endpoint<T> Post(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Post, path, shape, expectedMetaCode);

    public static Endpoint<T> Put(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Put, path, shape, expectedMetaCode);

    public static Endpoint<T> Delete(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Delete, path, shape, expectedMetaCode);

    /// <summary>An endpoint that faults the call before anything is sent; its other fields are never read.</summary>
    public static Endpoint<T> Rejected(Exception rejection) =>
        new(default, default, string.Empty, default, null, 0, default, null, rejection);

    /// <summary>Appends a query parameter; a null value is dropped. Enums must be passed as domain strings.</summary>
    public Endpoint<T> WithQuery(string name, object? value)
    {
        return value is null ? this : WithQueryValue(name, FormatQueryValue(value));
    }

    /// <summary>Appends a comma-separated list parameter such as <c>a,b,-c</c>; a null or empty list is dropped.</summary>
    public Endpoint<T> WithQuery(string name, IEnumerable<string>? values)
    {
        var list = values?.ToList();
        return list == null || list.Count == 0 ? this : WithQueryValue(name, string.Join(",", list));
    }

    public Endpoint<T> WithBody(object body) =>
        new(Resource, Verb, Path, Query, body, ExpectedMetaCode, Shape, _responseReader, Rejection);

    /// <summary>The same request whose response is read as <typeparamref name="T"/> and then converted by <paramref name="map"/>.</summary>
    public Endpoint<TResult> Map<TResult>(Func<T, TResult> map) =>
        new(Resource, Verb, Path, Query, Body, ExpectedMetaCode, Shape,
            (url, httpMethod, httpStatusCode, responseBody) =>
                map(EndpointWire.ReadResponse(this, url, httpMethod, httpStatusCode, responseBody)), Rejection);

    internal static string FormatQueryValue(object value)
    {
        return value switch
        {
            string text => text,
            bool flag => flag ? "true" : "false",
            Enum => throw new ArgumentException(
                "Enum query values must be passed as their BrickLink domain string.", nameof(value)),
            IEnumerable => throw new ArgumentException(
                "Collection query values must be passed as IEnumerable<string>.", nameof(value)),
            DateTime date => date.ToString("o", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToString("o", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private Endpoint<T> WithQueryValue(string name, string value) =>
        new(Resource, Verb, Path, Query.With(name, value), Body, ExpectedMetaCode, Shape, _responseReader, Rejection);

    private static Endpoint<T> Create(ApiResourceType resource, HttpVerb verb, string path,
        ResponseShape? requestedShape, int? expectedMetaCode)
    {
        var shape = EndpointWire.ResolveShape<T>(requestedShape);
        return new Endpoint<T>(resource, verb, path, default, null,
            expectedMetaCode ?? EndpointWire.DefaultExpectedMetaCode(verb, shape), shape, null, null);
    }
}
