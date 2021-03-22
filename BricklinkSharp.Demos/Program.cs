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
    internal static class Program
    {
        static async Task Main()
        {
            BricklinkClientConfiguration.Instance.TokenValue = "<Your Token>";
            BricklinkClientConfiguration.Instance.TokenSecret = "<Your Token Secret>";
            BricklinkClientConfiguration.Instance.ConsumerKey = "<Your Consumer Key>";
            BricklinkClientConfiguration.Instance.ConsumerSecret = "<Your Consumer Secret>";

            await CatalogDemos.GetItemImageDemo();
            CatalogDemos.GetPartImageForColorDemo();
            CatalogDemos.GetBookImageDemo();
            CatalogDemos.GetGearImageDemo();
            CatalogDemos.EnsureImageUrlSchemeDemo();
            await CatalogDemos.GetItemDemo();
            await CatalogDemos.GetSupersetsDemo();
            await CatalogDemos.GetSubsetsDemo();
            await CatalogDemos.GetPriceGuideDemo();
            await CatalogDemos.GetKnownColorsDemo();

            await ColorDemos.GetColorListDemo();
            await ColorDemos.GetColorDemo();

            await CategoryDemos.GetCategoryListDemo();
            await CategoryDemos.GetCategoryDemo();

            await InventoryDemos.CreateInventoriesDemo();
            var inventory = await InventoryDemos.CreateInventoryDemo();
            await InventoryDemos.UpdatedInventoryDemo(inventory.InventoryId);
            await InventoryDemos.DeleteInventoryDemo(inventory.InventoryId);
            await InventoryDemos.GetInventoryListDemo();

            await ItemMappingDemos.GetElementIdDemo();
            await ItemMappingDemos.GetItemNumberDemo();

            var shippingMethods = await SettingDemos.GetShippingMethodListDemo();
            var id = shippingMethods.First().MethodId;
            await SettingDemos.GetShippingMethodDemo(id);

            await PushNotificationDemos.GetNotificationsDemo();

            await MemberDemos.GetMemberRatingDemo();

            await FeedbackDemos.GetFeedbackListDemo();
            await FeedbackDemos.GetFeedbackDemo();
            var orderId = 123456789; //replace with a valid order ID.
            await FeedbackDemos.PostFeedbackDemo(orderId);
            var feedbackId = 123456789; //replace with a valid feedback ID.
            await FeedbackDemos.ReplyFeedbackDemo(feedbackId);

            await OrderDemos.GetOrdersDemo();
            await OrderDemos.GetOrderDemo();
            await OrderDemos.GetOrderItemsDemo();
            await OrderDemos.GetOrderMessagesDemo();
            await OrderDemos.GetOrderFeedbackDemo();
            await OrderDemos.UpdateOrderStatusDemo();
            await OrderDemos.UpdatePaymentStatusDemo();
            await OrderDemos.UpdateOrderDemo();

            await CouponDemos.GetCouponsDemo();
            var couponId = 123456789; //Must be a valid coupon ID.
            await CouponDemos.GetCouponDemo(couponId);
            await CouponDemos.DeleteCouponDemo(couponId);
            await CouponDemos.CreateCouponDemo();
            await CouponDemos.UpdateCouponDemo(couponId);

            await PartOutValueDemos.GetPartOutValueFromPageDemo();
            Console.ReadKey(true);
        }
    }
}
