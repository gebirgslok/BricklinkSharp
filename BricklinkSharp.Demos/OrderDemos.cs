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
using System.Threading.Tasks;
using BricklinkSharp.Client;

namespace BricklinkSharp.Demos
{
    internal static class OrderDemos
    {
        public static async Task GetOrderDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[0].OrderId;
            var order = await client.GetOrderAsync(orderId);

            PrintHelper.PrintAsJson(order);
        }

        public static async Task GetOrderItemsDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[0].OrderId;
            var itemsBatchList = await client.GetOrderItemsAsync(orderId);

            PrintHelper.PrintAsJson(itemsBatchList);
        }

        public static async Task GetOrderMessagesDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[0].OrderId;
            var messages = await client.GetOrderMessagesAsync(orderId);

            PrintHelper.PrintAsJson(messages);
        }

        public static async Task GetOrderFeedbackDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[orders.Length - 1].OrderId;
            var feedbackArray = await client.GetOrderFeedbackAsync(orderId);

            PrintHelper.PrintAsJson(feedbackArray);
        }

        public static async Task UpdateOrderStatusDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[^1].OrderId;
            var status = OrderStatus.Shipped;
            await client.UpdateOrderStatusAsync(orderId, status);

            Console.WriteLine($"Order status for order {orderId} successfully set to {status}.");
        }

        public static async Task UpdatePaymentStatusDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[^1].OrderId;
            var status = PaymentStatus.Received;
            await client.UpdatePaymentStatusAsync(orderId, status);

            Console.WriteLine($"Payment status for order {orderId} successfully set to {status}.");
        }

        public static async Task SendDriveThruDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[^1].OrderId;
            await client.SendDriveThruAsync(orderId, true);

            Console.WriteLine($"'Thank you, drive thru!' for order {orderId} successfully set sent.");
        }

        public static async Task UpdateOrderDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });
            var orderId = orders[^1].OrderId;
            var patch = new UpdateOrder();
            patch.Cost.Insurance = 2.5m;
            patch.Shipping.DateShipped = DateTime.Now;
            patch.Shipping.TrackingLink = "www.foo.bar";
            patch.Remarks = "Demp call :)";
            var updatedOrder = await client.UpdateOrderAsync(orderId, patch);

            PrintHelper.PrintAsJson(updatedOrder);
        }

        public static async Task GetOrdersDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
                {
                    OrderStatus.Paid
                });

            PrintHelper.PrintAsJson(orders);
        }
    }
}
