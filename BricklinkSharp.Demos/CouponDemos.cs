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
    internal static class CouponDemos
    {
        internal static async Task UpdateCouponDemo(int couponId)
        {
            using var client = BricklinkClientFactory.Build();
            var updateCoupon = new UpdateCoupon
            {
                DiscountType = DiscountType.Fixed,
                DiscountAmount = 2.0m
            };

            updateCoupon.AppliesTo.ItemType = ItemType.Part;

            var coupon = await client.UpdateCouponAsync(couponId, updateCoupon);

            PrintHelper.PrintAsJson(coupon);
        }

        internal static async Task CreateCouponDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var newCoupon = new NewCoupon("bluser", "Special gift for you")
            {
                DiscountType = DiscountType.Percentage,
                DiscountRate = 10
            };
            newCoupon.AppliesTo.ExceptOnSale = true;
            newCoupon.AppliesTo.RestrictionType = CouponRestrictionType.ApplyToSpecifiedItemType;
            newCoupon.AppliesTo.ItemType = null;

            var coupon = await client.CreateCouponAsync(newCoupon);

            PrintHelper.PrintAsJson(coupon);
        }

        internal static async Task DeleteCouponDemo(int couponId)
        {
            using var client = BricklinkClientFactory.Build();
            await client.DeleteCouponAsync(couponId);

            Console.WriteLine($"Coupon ID = {couponId} successfully deleted.");
        }

        internal static async Task GetCouponDemo(int couponId)
        {
            using var client = BricklinkClientFactory.Build();
            var coupon = await client.GetCouponAsync(couponId);

            PrintHelper.PrintAsJson(coupon);
        }

        internal static async Task GetCouponsDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var coupons = await client.GetCouponsAsync(Direction.Out,
                includedCouponStatusTypes: new List<CouponStatus> 
                {
                    CouponStatus.Open
                });

            PrintHelper.PrintAsJson(coupons);
        }
    }
}
