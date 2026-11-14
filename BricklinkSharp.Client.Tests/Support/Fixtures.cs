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
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;

namespace BricklinkSharp.Client.Tests.Support;

/// <summary>
/// Loads the embedded <c>Fixtures/*.json</c> response bodies and wraps them in the BrickLink envelope.
/// </summary>
internal static class Fixtures
{
    internal static string Json(string name)
    {
        var resourceName = $"Fixtures.{name}.json";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                           ?? throw new FileNotFoundException($"Embedded fixture '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static string Envelope(string dataJson) => Envelope(200, dataJson);

    internal static string Envelope(int code, string? dataJson = null) =>
        "{\"meta\":{\"description\":\"d\",\"message\":\"m\",\"code\":" + code + "}" +
        (dataJson == null ? "" : ",\"data\":" + dataJson) + "}";

    /// <summary>
    /// Loads a fixture and replaces one scalar, addressed by a dotted path such as <c>payment.status</c>.
    /// </summary>
    internal static string JsonWith(string name, string path, JsonNode? value)
    {
        var root = JsonNode.Parse(Json(name))!;
        var segments = path.Split('.');
        var parent = root;

        foreach (var segment in segments[..^1])
        {
            parent = parent[segment]!;
        }

        parent[segments[^1]] = value;
        return root.ToJsonString();
    }
}
