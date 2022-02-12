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
using System.Runtime.Serialization;
using System.Text;
// ReSharper disable UnusedMember.Local

namespace BricklinkSharp.Client;

public class BricklinkMissingCredentialsException : Exception
{
    internal BricklinkMissingCredentialsException(IReadOnlyList<string> missingParams) : base(BuildMessage(missingParams))
    {
    }

    private static string BuildMessage(IReadOnlyList<string> missingParams)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Missing credential parameter(s):");

        for (var i = 0; i < missingParams.Count; i++)
        {
            var missingParam = missingParams[i];
            var isLast = i == missingParams.Count - 1;
            builder.AppendLine($"Parameter = {missingParam}{(isLast ? "." : ", ")}");
        }

        return builder.ToString();
    }

    private BricklinkMissingCredentialsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}