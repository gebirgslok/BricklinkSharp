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

public class BricklinkInvalidParameterException : BricklinkException
{
    public Dictionary<string, object>? InvalidParameters { get; }

    public Type? DataObjectType { get; }

    internal BricklinkInvalidParameterException(Dictionary<string, object> invalidParameters,
        string? baseMessage = null,
        Type? dataObjectType = null) : base(BuildMessage(baseMessage, invalidParameters, dataObjectType))
    {
        InvalidParameters = invalidParameters;
        DataObjectType = dataObjectType;
    }

    private static string BuildMessage(string? baseMessage, Dictionary<string, object> invalidParameters, Type? dataObjectType)
    {
        var builder = new StringBuilder();

        if (baseMessage != null)
        {
            builder.AppendLine(baseMessage);
        }

        if (dataObjectType == null)
        {
            builder.AppendLine("The following parameters are invalid:");
        }
        else
        {
            builder.AppendLine($"The following parameters ({dataObjectType.Name}) are invalid:");
        }

        var count = 0;
        foreach (var o in invalidParameters)
        {
            count += 1;
            var separator = count < invalidParameters.Count ? ',' : '.';
            builder.AppendLine($"{o.Key}: {o.Value}{separator}");
        }

        return builder.ToString();
    }
}