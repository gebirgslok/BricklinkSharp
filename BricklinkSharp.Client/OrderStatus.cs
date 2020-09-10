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

namespace BricklinkSharp.Client
{
    public enum OrderStatus
    {
        [StringValue("PENDING")]
        Pending = 0,

        [StringValue("UPDATED")]
        Updated = 1,

        [StringValue("PROCESSING")]
        Processing = 2,

        [StringValue("READY")]
        Ready = 3,

        [StringValue("PAID")]
        Paid = 4,

        [StringValue("PACKED")]
        Packed = 5,

        [StringValue("SHIPPED")]
        Shipped = 6,

        [StringValue("RECEIVED")]
        Received = 7,

        [StringValue("COMPLETED")]
        Completed = 8,

        [StringValue("OCR")]
        Ocr = 9,

        [StringValue("NPB")]
        Npb = 10,

        [StringValue("NPX")]
        Npx = 11,

        [StringValue("NRS")]
        Nrs = 12,

        [StringValue("NSS")]
        Nss = 13,

        [StringValue("CANCELLED")]
        Cancelled = 14,

        [StringValue("PURGED")]
        Purged = 15
    }
}
