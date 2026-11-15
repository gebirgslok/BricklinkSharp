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
using System.Linq;
using System.Net.Http;
using BricklinkSharp.Client.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BricklinkSharp.Client.Tests;

public class EndpointsTests
{
    private static string PartSegment => ItemType.Part.ToDomainString();

    private static string Describe<T>(Endpoint<T> endpoint)
    {
        var query = string.Join("&", endpoint.Query.Pairs.Select(parameter => $"{parameter.Key}={parameter.Value}"));
        return query.Length == 0 ? endpoint.Path : $"{endpoint.Path}?{query}";
    }

    public sealed record ReadCase(string Name, string Expected, Func<string> Actual)
    {
        public override string ToString() => Name;
    }

    private static ReadCase Case<T>(string name, string expected, Endpoint<T> endpoint) =>
        new(name, expected, () => Describe(endpoint));

    private static IEnumerable<ReadCase> ReadCases()
    {
        var set = ItemType.Set.ToDomainString();

        yield return Case("CatalogItem", $"items/{PartSegment}/3001", Endpoints.CatalogItem(ItemType.Part, "3001"));
        yield return Case("CatalogImage", $"items/{PartSegment}/3001/images/5",
            Endpoints.CatalogImage(ItemType.Part, "3001", 5));
        yield return Case("Supersets", $"items/{PartSegment}/3001/supersets",
            Endpoints.Supersets(ItemType.Part, "3001", 0));
        yield return Case("SupersetsWithColor", $"items/{PartSegment}/3001/supersets?color_id=5",
            Endpoints.Supersets(ItemType.Part, "3001", 5));
        yield return Case("SubsetsWithoutColor", $"items/{set}/10000-1/subsets?box=true&break_minifigs=false",
            Endpoints.Subsets(ItemType.Set, "10000-1", 0, true, null, false, null));
        yield return Case("SubsetsAllFlags",
            $"items/{set}/10000-1/subsets?color_id=5&box=true&instruction=false&break_minifigs=true&break_subsets=false",
            Endpoints.Subsets(ItemType.Set, "10000-1", 5, true, false, true, false));
        yield return Case("PriceGuide",
            $"items/{PartSegment}/3001/price?color_id=5&guide_type={PriceGuideSource.Sold.ToDomainString()}" +
            $"&new_or_used={Condition.Used.ToDomainString()}&country_code=DE&region=europe&currency_code=EUR" +
            $"&vat={VatOption.Include.ToDomainString()}",
            Endpoints.PriceGuide(ItemType.Part, "3001", 5, PriceGuideSource.Sold, Condition.Used, "DE", "europe",
                "EUR", VatOption.Include));
        yield return Case("PriceGuideWithoutColor", $"items/{PartSegment}/3001/price",
            Endpoints.PriceGuide(ItemType.Part, "3001", 0, null, null, null, null, null, null));
        yield return Case("KnownColors", $"items/{PartSegment}/3001/colors",
            Endpoints.KnownColors(ItemType.Part, "3001"));
        yield return Case("Colors", "colors", Endpoints.Colors());
        yield return Case("Color", "colors/5", Endpoints.Color(5));
        yield return Case("Categories", "categories", Endpoints.Categories());
        yield return Case("Category", "categories/7", Endpoints.Category(7));
        yield return Case("ItemMappingsByItemNumber", "item_mapping/part/3001?color_id=5",
            Endpoints.ItemMappingsByItemNumber("3001", 5));
        yield return Case("ItemMappingsByItemWithoutColor", "item_mapping/part/3001",
            Endpoints.ItemMappingsByItemNumber("3001", null));
        yield return Case("ItemMappingsByElementId", "item_mapping/300101",
            Endpoints.ItemMappingsByElementId("300101"));
        yield return Case("Inventories", $"inventories?item_type={PartSegment},-{set}&category_id=1,2,-3",
            Endpoints.Inventories(new[] { ItemType.Part }, new[] { ItemType.Set }, null, null, new[] { 1, 2 },
                new[] { 3 }, null, null));
        yield return Case("InventoriesWithStatusAndColor",
            $"inventories?status={InventoryStatus.Available.ToDomainString()},-{InventoryStatus.Reserved.ToDomainString()}&color_id=4",
            Endpoints.Inventories(null, null, new[] { InventoryStatus.Available }, new[] { InventoryStatus.Reserved },
                null, null, new[] { 4 }, null));
        yield return Case("Inventory", "inventories/9", Endpoints.Inventory(9));
        yield return Case("ShippingMethods", "settings/shipping_methods", Endpoints.ShippingMethods());
        yield return Case("ShippingMethod", "settings/shipping_methods/3", Endpoints.ShippingMethod(3));
        yield return Case("PushNotifications", "notifications", Endpoints.PushNotifications());
        yield return Case("MemberRating", "members/bob/ratings", Endpoints.MemberRating("bob"));
        yield return Case("Feedbacks", "feedback", Endpoints.Feedbacks(null));
        yield return Case("FeedbacksIn", $"feedback?direction={Direction.In.ToDomainString()}",
            Endpoints.Feedbacks(Direction.In));
        yield return Case("Feedback", "feedback/4", Endpoints.Feedback(4));
        yield return Case("Orders",
            $"orders?direction={Direction.Out.ToDomainString()}&filed=true",
            Endpoints.Orders(Direction.Out, null, null, true));
        yield return Case("OrdersWithStatusFlags",
            $"orders?direction={Direction.In.ToDomainString()}&status={OrderStatus.Pending.ToDomainString()}," +
            $"-{OrderStatus.Cancelled.ToDomainString()}&filed=false",
            Endpoints.Orders(Direction.In, new[] { OrderStatus.Pending }, new[] { OrderStatus.Cancelled }, false));
        yield return Case("Order", "orders/11", Endpoints.Order(11));
        yield return Case("OrderItems", "orders/11/items", Endpoints.OrderItems(11));
        yield return Case("OrderMessages", "orders/11/messages", Endpoints.OrderMessages(11));
        yield return Case("OrderFeedbacks", "orders/11/feedback", Endpoints.OrderFeedbacks(11));
        yield return Case("Coupons", $"coupons?direction={Direction.Out.ToDomainString()}",
            Endpoints.Coupons(Direction.Out, null, null));
        yield return Case("CouponsWithStatusFlags",
            $"coupons?direction={Direction.In.ToDomainString()}&status={CouponStatus.Open.ToDomainString()}," +
            $"-{CouponStatus.Expired.ToDomainString()}",
            Endpoints.Coupons(Direction.In, new[] { CouponStatus.Open }, new[] { CouponStatus.Expired }));
        yield return Case("Coupon", "coupons/2", Endpoints.Coupon(2));
    }

    [TestCaseSource(nameof(ReadCases))]
    public void Should_DescribeExpectedPathAndQuery_When_ReadEndpointIsBuilt(ReadCase readCase)
    {
        ClassicAssert.AreEqual(readCase.Expected, readCase.Actual());
    }

    [Test]
    public void Should_UseGetAndNoBody_When_ReadEndpointIsBuilt()
    {
        var endpoint = Endpoints.Orders(Direction.In, null, null, false);

        ClassicAssert.AreEqual(HttpVerb.Get, endpoint.Verb);
        ClassicAssert.IsNull(endpoint.Body);
        ClassicAssert.IsNull(endpoint.Rejection);
        ClassicAssert.AreEqual(200, endpoint.ExpectedMetaCode);
    }

    [Test]
    public void Should_RequireNonEmptyArray_When_ColorsAreRequested()
    {
        ClassicAssert.AreEqual(ResponseShape.ArrayNonEmpty, Endpoints.Colors().Shape);
        ClassicAssert.AreEqual(ResponseShape.ArrayOrEmpty, Endpoints.PushNotifications().Shape);
    }

    [Test]
    public void Should_MapBatchesToList_When_OrderItemsAreRead()
    {
        var endpoint = Endpoints.OrderItems(11);
        const string body = "{\"meta\":{\"code\":200},\"data\":[[{\"inventory_id\":1},{\"inventory_id\":2}],[{\"inventory_id\":3}]]}";

        var batches = EndpointWire.ReadResponse(endpoint, new Uri("https://example.test/orders/11/items"), HttpMethod.Get, 200, body);

        ClassicAssert.AreEqual(ResponseShape.ArrayOrEmpty, endpoint.Shape);
        CollectionAssert.AreEqual(new[] { 2, 1 }, batches.Select(batch => batch.Length));
    }
}
