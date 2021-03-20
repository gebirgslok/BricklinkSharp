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

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

namespace BricklinkSharp.Client
{
    public class BricklinkPartOutRequestErrorException : BricklinkException
    {
        public List<string> Errors { get; }

        internal BricklinkPartOutRequestErrorException(List<string> errors, string url) : base(BuildMessage(errors), url, HttpMethod.Get)
        {
            Errors = errors;
        }

        internal BricklinkPartOutRequestErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Errors = new List<string>();

            var count = info.GetInt32("Count");

            for (var i = 0; i < count; i++)
            {
                var message = info.GetString($"Error{i}");

                if (message != null)
                {
                    Errors.Add(message);
                }
            }
        }

        private static string BuildMessage(List<string> errors)
        {
            var builder = new StringBuilder("The Part-Out value request returned a global error page. The following error(s) occurred:");

            for (var i = 0; i < errors.Count; i++)
            {
                builder.AppendLine();
                builder.Append($"{i} - {errors[i]}");
            }

            return builder.ToString();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Count", Errors.Count);
            
            for (var i = 0; i < Errors.Count; i++)
            {
                info.AddValue($"Error{i}", Errors[i]);
            }

            base.GetObjectData(info, context);
        }
    }
}