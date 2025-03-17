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
using System.Text;

namespace BricklinkSharp.Client;

public class BricklinkInvalidParameterException : Exception
{
    public Dictionary<string, object>? InvalidParameters { get; }

    public Type? TypeOfDataObject { get; }

    internal BricklinkInvalidParameterException(Dictionary<string, object> invalidParameters,
        string? baseMessage = null,
        Type? typeOfDataObject = null) : base(BuildMessage(baseMessage, invalidParameters, typeOfDataObject))
    {
        InvalidParameters = invalidParameters;
        TypeOfDataObject = typeOfDataObject;
    }

    private static string BuildMessage(string? baseMessage, Dictionary<string, object> invalidParameters, Type? typeOfDataObject)
    {
        var builder = new StringBuilder();

        if (baseMessage != null)
        {
            builder.AppendLine(baseMessage);
        }

        if (typeOfDataObject == null)
        {
            builder.AppendLine("The following parameters are invalid:");
        }
        else
        {
            builder.AppendLine($"The following parameters ({typeOfDataObject.Name}) are invalid:");
        }

        var count = 0;
        foreach (var o in invalidParameters)
        {
            var separator = count <= invalidParameters.Count - 1 ? ',' : '.';
            builder.AppendLine($"{o.Key}: {o.Value}{separator}");
            count += 1;
        }

        return builder.ToString();
    }
}