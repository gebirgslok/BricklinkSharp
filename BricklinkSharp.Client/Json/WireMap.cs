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

namespace BricklinkSharp.Client.Json;

/// <summary>
/// The single owner of one enum's wire format: both directions come from the same table.
/// </summary>
internal sealed class WireMap<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<TEnum, string> _wireByValue = new();
    private readonly Dictionary<string, TEnum> _valueByWire;

    private WireMap(IEnumerable<(TEnum Value, string Wire)> pairs, IEqualityComparer<string> wireComparer)
    {
        _valueByWire = new Dictionary<string, TEnum>(wireComparer);

        foreach (var (value, wire) in pairs)
        {
            if (_wireByValue.ContainsKey(value))
            {
                throw new ArgumentException($"{typeof(TEnum).Name}.{value} is mapped twice.", nameof(pairs));
            }

            if (_valueByWire.ContainsKey(wire))
            {
                throw new ArgumentException($"Wire value '{wire}' of {typeof(TEnum).Name} is mapped twice.", nameof(pairs));
            }

            _wireByValue.Add(value, wire);
            _valueByWire.Add(wire, value);
        }
    }

    /// <summary>Explicit literals, read case-sensitively.</summary>
    internal static WireMap<TEnum> Of(params (TEnum Value, string Wire)[] pairs) =>
        new(pairs, StringComparer.Ordinal);

    /// <summary>Explicit literals, read case-insensitively.</summary>
    internal static WireMap<TEnum> OfIgnoringCase(params (TEnum Value, string Wire)[] pairs) =>
        new(pairs, StringComparer.OrdinalIgnoreCase);

    /// <summary>Explicit literals, read ignoring case and underscores (<c>UNSORTEDLOT</c> equals <c>unsorted_lot</c>).</summary>
    internal static WireMap<TEnum> OfIgnoringCaseAndUnderscores(params (TEnum Value, string Wire)[] pairs) =>
        new(pairs, new CaseAndUnderscoreInsensitiveComparer());

    /// <summary>The declared member name, for example <c>Praise</c>; read case-insensitively.</summary>
    internal static WireMap<TEnum> ByName() => ByName(name => name);

    /// <summary>The upper-case member name, for example <c>PENDING</c>; read case-insensitively.</summary>
    internal static WireMap<TEnum> ByNameUpper() => ByName(name => name.ToUpperInvariant());

    /// <summary>The lower-case member name, for example <c>sold</c>; read case-insensitively.</summary>
    internal static WireMap<TEnum> ByNameLower() => ByName(name => name.ToLowerInvariant());

    internal string ToWire(TEnum value)
    {
        if (_wireByValue.TryGetValue(value, out var wire))
        {
            return wire;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, $"{typeof(TEnum).Name} has no wire value.");
    }

    internal bool TryFromWire(string? wire, out TEnum value)
    {
        if (wire != null)
        {
            return _valueByWire.TryGetValue(wire, out value);
        }

        value = default;
        return false;
    }

    internal TEnum FromWire(string? wire)
    {
        if (TryFromWire(wire, out var value))
        {
            return value;
        }

        throw new FormatException($"'{wire}' is not a known wire value of {typeof(TEnum).Name}.");
    }

    private static WireMap<TEnum> ByName(Func<string, string> toWire) =>
        new(Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(value => (value, toWire(value.ToString()))),
            StringComparer.OrdinalIgnoreCase);

    private sealed class CaseAndUnderscoreInsensitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => Canonical(x) == Canonical(y);

        public int GetHashCode(string obj) => Canonical(obj).GetHashCode();

        private static string Canonical(string? wire) => (wire ?? string.Empty).Replace("_", string.Empty).ToUpperInvariant();
    }
}
