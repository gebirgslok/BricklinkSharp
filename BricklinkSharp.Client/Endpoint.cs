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
using System.Threading;
using System.Threading.Tasks;

namespace BricklinkSharp.Client;

/// <summary>
/// What to send and how to read the response for one API operation. Pure data: building one does no I/O.
/// A <see cref="Rejection"/> means the arguments were invalid and nothing may be sent.
/// </summary>
internal sealed class Endpoint<T>
{
    private static readonly IReadOnlyList<KeyValuePair<string, string>> NoQuery =
        Array.Empty<KeyValuePair<string, string>>();

    internal Endpoint(ApiResourceType resource, HttpVerb verb, string path,
        IReadOnlyList<KeyValuePair<string, string>> query, object? body, int expectedMetaCode,
        ResponseShape shape, Func<BricklinkRequestExecutor, Endpoint<T>, BricklinkCredentials?, CancellationToken, Task<T>>? project,
        Exception? rejection)
    {
        Resource = resource;
        Verb = verb;
        Path = path;
        Query = query;
        Body = body;
        ExpectedMetaCode = expectedMetaCode;
        Shape = shape;
        Project = project;
        Rejection = rejection;
    }

    public ApiResourceType Resource { get; }

    public HttpVerb Verb { get; }

    public string Path { get; }

    public IReadOnlyList<KeyValuePair<string, string>> Query { get; }

    public object? Body { get; }

    public int ExpectedMetaCode { get; }

    public ResponseShape Shape { get; }

    /// <summary>Reads the response as its wire type and converts it; null when the response is read as <typeparamref name="T"/>.</summary>
    internal Func<BricklinkRequestExecutor, Endpoint<T>, BricklinkCredentials?, CancellationToken, Task<T>>? Project { get; }

    public Exception? Rejection { get; }

    public static Endpoint<T> Get(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Get, path, shape, expectedMetaCode);

    public static Endpoint<T> Post(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Post, path, shape, expectedMetaCode);

    public static Endpoint<T> Put(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Put, path, shape, expectedMetaCode);

    public static Endpoint<T> Delete(ApiResourceType resource, string path, ResponseShape? shape = null,
        int? expectedMetaCode = null) => Create(resource, HttpVerb.Delete, path, shape, expectedMetaCode);

    /// <summary>A GET whose response is read as <typeparamref name="TWire"/> and then converted by <paramref name="project"/>.</summary>
    public static Endpoint<T> GetProjected<TWire>(ApiResourceType resource, string path, Func<TWire, T> project)
    {
        var shape = BricklinkResponseParser.ResolveShape<TWire>(null);
        return new Endpoint<T>(resource, HttpVerb.Get, path, NoQuery, null,
            BricklinkResponseParser.DefaultExpectedMetaCode(HttpVerb.Get, shape), shape,
            async (runner, self, credentials, cancellationToken) =>
                project(await runner.SendWithoutProjectionAsync(self.Retarget<TWire>(), credentials, cancellationToken)
                    .ConfigureAwait(false)), null);
    }

    public static Endpoint<T> Rejected(Exception rejection) =>
        new(default, default, string.Empty, NoQuery, null, 0, default, null, rejection);

    /// <summary>Appends a query parameter; a null value is dropped. Enums must be passed as domain strings.</summary>
    public Endpoint<T> WithQuery(string name, object? value)
    {
        if (value is null)
        {
            return this;
        }

        var parameter = new KeyValuePair<string, string>(name, FormatQueryValue(value));
        return new Endpoint<T>(Resource, Verb, Path, Query.Append(parameter).ToArray(), Body, ExpectedMetaCode,
            Shape, Project, Rejection);
    }

    public Endpoint<T> WithBody(object body) =>
        new(Resource, Verb, Path, Query, body, ExpectedMetaCode, Shape, Project, Rejection);

    /// <summary>The same request, read as <typeparamref name="TOther"/> without projection.</summary>
    internal Endpoint<TOther> Retarget<TOther>() =>
        new Endpoint<TOther>(Resource, Verb, Path, Query, Body, ExpectedMetaCode, Shape, null, null);

    internal string BuildUrl(Uri baseUri)
    {
        var builder = new StringBuilder();
        builder.Append(baseUri.AbsoluteUri.TrimEnd('/'));
        builder.Append('/');
        builder.Append(Path.TrimStart('/'));

        for (var i = 0; i < Query.Count; i++)
        {
            builder.Append(i == 0 ? '?' : '&');
            builder.Append(Uri.EscapeDataString(Query[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(Query[i].Value));
        }

        return builder.ToString();
    }

    internal static string FormatQueryValue(object value)
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

    private static Endpoint<T> Create(ApiResourceType resource, HttpVerb verb, string path,
        ResponseShape? requestedShape, int? expectedMetaCode)
    {
        var shape = BricklinkResponseParser.ResolveShape<T>(requestedShape);
        return new Endpoint<T>(resource, verb, path, NoQuery, null,
            expectedMetaCode ?? BricklinkResponseParser.DefaultExpectedMetaCode(verb, shape), shape, null, null);
    }
}
