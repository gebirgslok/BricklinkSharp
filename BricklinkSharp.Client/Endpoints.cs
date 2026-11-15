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
using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client;

/// <summary>One pure factory per API operation. Policy for an endpoint (path, query, response shape) lives here.</summary>
internal static partial class Endpoints
{
    /// <summary>Query value for filters with included and excluded values, e.g. <c>a,b,-c</c>; null when both are empty.</summary>
    private static string? IncludeExclude<T>(IEnumerable<T>? includes, IEnumerable<T>? excludes,
        Func<T, string> toWire)
    {
        var parameters = (includes ?? Enumerable.Empty<T>()).Select(toWire)
            .Concat((excludes ?? Enumerable.Empty<T>()).Select(value => $"-{toWire(value)}"))
            .ToList();

        return parameters.Any() ? parameters.ToConcatenatedString() : null;
    }

    /// <summary>A zero or negative color id means "any color" and is not sent.</summary>
    private static int? ColorIdOrNull(int colorId) => colorId > 0 ? colorId : null;

    /// <summary>Builds the endpoint only if <paramref name="throwIfInvalid"/> does not throw; otherwise the endpoint is a rejection and nothing is sent.</summary>
    private static Endpoint<T> RejectingInvalid<T>(Action throwIfInvalid, Func<Endpoint<T>> buildEndpoint)
    {
        try
        {
            throwIfInvalid();
        }
        catch (Exception exception)
        {
            return Endpoint<T>.Rejected(exception);
        }

        return buildEndpoint();
    }
}
