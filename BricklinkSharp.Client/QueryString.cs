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

internal readonly struct QueryString
{
    private readonly KeyValuePair<string, string>[]? _pairs;

    private QueryString(KeyValuePair<string, string>[] pairs)
    {
        _pairs = pairs;
    }

    public IReadOnlyList<KeyValuePair<string, string>> Pairs =>
        _pairs ?? Array.Empty<KeyValuePair<string, string>>();

    public QueryString With(string name, string value)
    {
        var pairs = new KeyValuePair<string, string>[Pairs.Count + 1];

        for (var i = 0; i < pairs.Length - 1; i++)
        {
            pairs[i] = Pairs[i];
        }

        pairs[pairs.Length - 1] = new KeyValuePair<string, string>(name, value);
        return new QueryString(pairs);
    }

    public string Encode()
    {
        return string.Join("&", Pairs.Select(p => $"{Rfc3986.Encode(p.Key)}={Rfc3986.Encode(p.Value)}"));
    }
}
