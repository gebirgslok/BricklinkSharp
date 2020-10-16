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
using System.Linq;
using System.Threading.Tasks;
using BricklinkSharp.Client;

namespace BricklinkSharp.Demos
{
    internal static class FeedbackDemos
    {
        public static async Task GetFeedbackListDemo()
        {
            using var client = BricklinkClientFactory.Build();

            var feedbackInList = await client.GetFeedbackListAsync(FeedbackDirection.In);
            Console.WriteLine("Received feedback ('In'):");
            PrintHelper.PrintAsJson(feedbackInList);

            var feedbackOutList = await client.GetFeedbackListAsync(FeedbackDirection.Out);
            Console.WriteLine("Posted feedback ('Out'):");
            PrintHelper.PrintAsJson(feedbackOutList);
        }

        public static async Task GetFeedbackDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var feedbackInList = await client.GetFeedbackListAsync(FeedbackDirection.In);
            var id = feedbackInList.First().FeedbackId;

            var feedback = await client.GetFeedbackAsync(id);

            PrintHelper.PrintAsJson(feedback);
        }

        public static async Task PostFeedbackDemo(int orderId)
        {
            using var client = BricklinkClientFactory.Build();
            var feedback = await client.PostFeedbackAsync(orderId, RatingType.Praise, "Awesome transaction, praise!");

            Console.WriteLine("Posted feedback:");
            PrintHelper.PrintAsJson(feedback);
        }

        public static async Task ReplyFeedbackDemo(int feedbackId)
        {
            using var client = BricklinkClientFactory.Build();
            await client.ReplyFeedbackAsync(feedbackId, "Thank you for your kind comment.");
        }
    }
}