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

using BricklinkSharp.Client.Extensions;
using NUnit.Framework;

namespace BricklinkSharp.Client.Tests.Extensions
{
    public class EnumExtensionsTests
    {
        [Test]
        public void GetStringValueOrDefault_ItemType()
        {
            Assert.AreEqual("minifig", ItemType.Minifig.GetStringValueOrDefault());
            Assert.AreEqual("part", ItemType.Part.GetStringValueOrDefault());
            Assert.AreEqual("book", ItemType.Book.GetStringValueOrDefault());
            Assert.AreEqual("catalog", ItemType.Catalog.GetStringValueOrDefault());
            Assert.AreEqual("gear", ItemType.Gear.GetStringValueOrDefault());
            Assert.AreEqual("instruction", ItemType.Instruction.GetStringValueOrDefault());
            Assert.AreEqual("original_box", ItemType.OriginalBox.GetStringValueOrDefault());
            Assert.AreEqual("unsorted_lot", ItemType.UnsortedLot.GetStringValueOrDefault());
        }
    }
}