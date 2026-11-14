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
using System.Linq;

namespace BricklinkSharp.Client;

/// <summary>
/// What to send and how to read the response for one API operation. Pure data: building one does no I/O.
/// A <see cref="Rejection"/> means the arguments were invalid and nothing may be sent.
/// </summary>
internal sealed class Endpoint<T>
{
    private static readonly IReadOnlyList<KeyValuePair<string, string>> NoQuery =
        Array.Empty<KeyValuePair<string, string>>();

    private Endpoint(ApiResourceType resource, HttpVerb verb, string path,
        IReadOnlyList<KeyValuePair<string, string>> query, object? body, int expectedMetaCode,
        ResponseShape shape, BricklinkException? rejection)
    {
        Resource = resource;
        Verb = verb;
        Path = path;
        Query = query;
        Body = body;
        ExpectedMetaCode = expectedMetaCode;
        Shape = shape;
        Rejection = rejection;
    }

    public ApiResourceType Resource { get; }

    public HttpVerb Verb { get; }

    public string Path { get; }

    public IReadOnlyList<KeyValuePair<string, string>> Query { get; }

    public object? Body { get; }

    public int ExpectedMetaCode { get; }

    public ResponseShape Shape { get; }

    public BricklinkException? Rejection { get; }

    public static Endpoint<T> Get(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Get, path, shape, expectedMetaCode);

    public static Endpoint<T> Post(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Post, path, shape, expectedMetaCode);

    public static Endpoint<T> Put(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Put, path, shape, expectedMetaCode);

    public static Endpoint<T> Delete(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Delete, path, shape, expectedMetaCode);

    public static Endpoint<T> Rejected(BricklinkException rejection) =>
        new(default, default, string.Empty, NoQuery, null, 0, default, rejection);

    /// <summary>Appends a query parameter; a null value is dropped. Enums must be passed as domain strings.</summary>
    public Endpoint<T> WithQuery(string name, object? value)
    {
        if (value is null)
        {
            return this;
        }

        var parameter = new KeyValuePair<string, string>(name, BricklinkRequest.FormatQueryValue(value));
        return new Endpoint<T>(Resource, Verb, Path, Query.Append(parameter).ToArray(), Body, ExpectedMetaCode,
            Shape, Rejection);
    }

    public Endpoint<T> WithBody(object body) =>
        new(Resource, Verb, Path, Query, body, ExpectedMetaCode, Shape, Rejection);

    internal BricklinkRequest ToRequest(BricklinkCredentials? credentials)
    {
        var request = new BricklinkRequest(Resource, Verb, Path);

        foreach (var parameter in Query)
        {
            request = request.WithQueryParameter(parameter.Key, parameter.Value);
        }

        return request
            .WithBody(Body)
            .WithCredentials(credentials)
            .WithExpectedMetaCode(ExpectedMetaCode)
            .WithShape(Shape);
    }

    private static Endpoint<T> Create(ApiResourceType resource, HttpVerb verb, string path,
        ResponseShape? requestedShape, int? expectedMetaCode)
    {
        var shape = BricklinkResponseParser.ResolveShape<T>(requestedShape);
        return new Endpoint<T>(resource, verb, path, NoQuery, null,
            expectedMetaCode ?? BricklinkResponseParser.DefaultExpectedMetaCode(verb, shape), shape, null);
    }
}
