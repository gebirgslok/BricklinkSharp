# BricklinkSharp

[![NuGet](https://img.shields.io/nuget/v/BricklinkSharp?color=blue)](https://www.nuget.org/packages/BricklinkSharp/)
[![CI](https://github.com/gebirgslok/BricklinkSharp/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/gebirgslok/BricklinkSharp/actions/workflows/ci.yml)

## :star: Leave a star

If this repo helps you out then why not give it a star. I'll appreciate.

## Introduction

BricklinkSharp is a strongly-typed, easy-to-use C# client for the [bricklink](https://www.bricklink.com/v2/main.page) marketplace that gets you started with just a few lines of code. It features OAuth1 authentication, error handling and parsing of JSON data into typed instances.
It supports all .NET platforms compatible with _.NET standard 2.0_.

## Related projects

- [RebrickableSharp](https://github.com/gebirgslok/RebrickableSharp) - Easy-to-use C# client for the Rebrickable (LEGO) API.

## Changelog

> :information_source: At the moment the project is assumed to be complete, nevertheless it will continue to be actively maintained to fix bugs and respond to API changes. If you encounter bugs or have suggestions for additional features then you can simply open an issue or submit a pull request.

See [CHANGELOG.md](CHANGELOG.md) for the release history.

## Migrating to 2.0.0

2.0.0 is a deliberate clean break: names follow one set of rules (see [ADR 0001](docs/adr/0001-clean-break-2-0-0.md)) and there are no `[Obsolete]` forwarders. JSON wire names are unchanged. The complete old → new table is in [CHANGELOG.md](CHANGELOG.md).

- `Rating` now names the verdict of a single feedback (formerly `RatingType`). The aggregated counts class is `FeedbackCounts` (formerly `Rating`).
- `Get<Things>Async` methods are plural (`GetColorsAsync`, `GetFeedbacksAsync`, ...).
- Parameters naming an item number are `itemNumber`; date properties end in `On` (`OrderedOn`, `PaidOn`, ...).
- `OrderDirection` is merged into `Direction`.

## Get started

### Demo project

Check out the [demo project](https://github.com/gebirgslok/BricklinkSharp/tree/main/BricklinkSharp.Demos) for full-featured examples.

### Prerequisites

You need to be registered on [bricklink](https://www.bricklink.com/v2/main.page) as a **seller**. Then, use this [link](https://www.bricklink.com/v2/api/register_consumer.page) to add an access token. Add your client IP address (if applicable) or _0.0.0.0_ for both IP address and mask to be able to make API requests from any IP.

### Install NuGet package

#### Package Manager Console

```
Install-Package BricklinkSharp
```

#### Command Line

```
nuget install BricklinkSharp
```

### Setup credentials

```csharp
BricklinkClientConfiguration.Instance.TokenValue = "<Your Token>";
BricklinkClientConfiguration.Instance.TokenSecret = "<Your Token Secret>";
BricklinkClientConfiguration.Instance.ConsumerKey = "<Your Consumer Key>";
BricklinkClientConfiguration.Instance.ConsumerSecret = "<Your Consumer Secret>";
```

### IBricklinkClient

```csharp
var client = BricklinkClientFactory.Build();
```

#### Usage recommendation

It's recommended to create and use one _IBricklinkClient_ client throughout the lifetime of your application.

In applications using an IoC container you may register the _IBricklinkClient_ as a service and inject it into consuming instances (e.g. controllers).
See the below examples to register the _IBricklinkClient_ as single instance (Singleton).

#### [Autofac](https://autofac.org/) example

```csharp
containerBuilder.Register(c => BricklinkClientFactory.Build())
	.As<IBricklinkClient>()
	.SingleInstance();
```

#### [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/de-de/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0) example

```csharp
services.AddSingleton(typeof(IBricklinkClient), provider =>
{
    return BricklinkClientFactory.Build();
});
```

### Item Catalog

#### Get catalog item

```csharp
var catalogItem = await client.GetCatalogItemAsync(ItemType.Part, "6089");
```

#### Get catalog image

```csharp
var catalogImage = await client.GetCatalogImageAsync(ItemType.OriginalBox, "1-12", 0);
```

#### Get part image for color ID

```csharp
var colorId = 34; //Lime.
var uri = client.GetPartImageForColor("43898pb006", 34);
```

#### Get minifigure image

```csharp
var uri = client.GetMinifigImage("sw1093");
```

#### Get set image

```csharp
var uri = client.GetSetImage("6090-1");
```

#### Get book image

```csharp
var uri = client.GetBookImage("5002772");
```

#### Get gear image

```csharp
var uri = client.GetGearImage("BioGMC041");
```

#### Get catalog image

```csharp
var uri = client.GetCatalogImage("c58dk2");
```

#### Get instruction image

```csharp
var uri = client.GetInstructionImage("1518-1");
```

#### Get original box image

```csharp
var uri = client.GetOriginalBoxImage("2964-1");
```

#### Ensure image url scheme

```csharp
var uri = client.EnsureImageUrlScheme("//img.bricklink.com/ItemImage/PN/34/43898pb006.png", "https");
Console.WriteLine(uri.ToString());
//Prints: https://img.bricklink.com/ItemImage/PN/34/43898pb006.png
```

#### Get supersets

```csharp
var supersets = await client.GetSupersetsAsync(ItemType.Minifig, "aqu004", 0);
```

#### Get subsets

```csharp
var subsets = await client.GetSubsetsAsync(ItemType.Set, "1095-1", breakMinifigs: false);
```

#### Get price guide

```csharp
var priceGuide = await client.GetPriceGuideAsync(ItemType.Part, "3003", colorId: 1, source: PriceGuideSource.Sold, condition: Condition.Used);
```

#### Get known colors

```csharp
var knownColors = await client.GetKnownColorsAsync(ItemType.Part, "3006");
```

### Color

#### Get color list

```csharp
var colors = await client.GetColorsAsync();
```

#### Get color

```csharp
var color = await client.GetColorAsync(15);
```

### Category

#### Get category list

```csharp
var categories = await client.GetCategoriesAsync();
```

#### Get category

```csharp
var category = await client.GetCategoryAsync(1);
```

### User Inventory

#### Get inventory list

```csharp
//Include only parts and minifigs.
var includedTypes = new List<ItemType> { ItemType.Part, ItemType.Minifig };
//Exclude all inventories which are unavailable.
var excludedStatusFlags = new List<InventoryStatus> { Unavailable };
var inventories = await client.GetInventoriesAsync(includedTypes, excludedStatusFlags: excludedStatusFlags);
```

#### Create inventory

```csharp
var newInventory = new NewInventory
{
	ColorId = 1,
	Condition = Condition.Used,
	Item = new Item
	{
		ItemNumber = "3003",
		Type = ItemType.Part
	},
	Quantity = 5,
	UnitPrice = 0.01M,
	Description = "Good used condition"
};
var inventory = await client.CreateInventoryAsync(newInventory);
```

#### Create inventories

```csharp
var newInventories = new NewInventory[] { //fill with inventories... };
//Note that there will be no response data.
await client.CreateInventoriesAsync(newInventories);
```

#### Update inventory

```csharp
var inventoryList = await client.GetInventoriesAsync();
var id = inventoryList.First().Id;
var updateInventory = new UpdateInventory { ChangedQuantity = 21, Remarks = "Remarks added." };
var inventory = await client.UpdateInventoryAsync(id, updateInventory);
```

#### Delete inventory

```csharp
var inventoryList = await client.GetInventoriesAsync();
var id = inventoryList.First().Id;
await client.DeleteInventoryAsync(id);
```

### Item Mapping

#### Get item mappings by item

The method returns an array of _ItemMapping_ objects. If a color ID is specified the array will contain just one element. Otherwise the array will contain mappings for every available color.

```csharp
var itemMappings = await client.GetItemMappingsByItemAsync("3003", 1);
```

#### Get item mappings by element ID

```csharp
var itemMapping = await client.GetItemMappingsByElementIdAsync("300301");
```

### Push Notification

#### Get notifications

```csharp
var notifications = await client.GetPushNotificationsAsync();
```

### Setting

#### Get shipping method list

```csharp
var shippingMethods = await client.GetShippingMethodsAsync();
```

#### Get shipping method

```csharp
var shippingMethod = await client.GetShippingMethodAsync(123);
```

### Member

#### Get member rating

```csharp
var memberRating = await client.GetMemberRatingAsync("bluser");
```

### Feedback

#### Get feedbacks

```csharp
var feedbackInList = await client.GetFeedbacksAsync(Direction.In);
var feedbackOutList = await client.GetFeedbacksAsync(Direction.Out);
var feedbackListAll = await client.GetFeedbacksAsync();
```

#### Get feedback

```csharp
var feedbackListAll = await client.GetFeedbacksAsync()
var id = feedbackListAll.First().Id;
var feedback = await client.GetFeedbackAsync(id);
```

#### Post feedback

```csharp
var orderId = 1234567; //Must be a valid order ID.
var feedback = await client.PostFeedbackAsync(orderId, Rating.Praise, "Awesome transaction, praise!");
```

#### Reply to feedback

```csharp
var feedbackId = 1234567; //Must be a valid feedback ID.
await client.ReplyToFeedbackAsync(feedbackId, "Thank you for your kind comment.");
```

### Order

#### Get Orders

```csharp
//Exclude all purged orders.
var orders = await client.GetOrdersAsync(Direction.Out, excludedStatusFlags: new List<OrderStatus>
{
	OrderStatus.Purged
});
```

```csharp
//Only paid orders that must be shipped.
var orders = await client.GetOrdersAsync(Direction.Out, includedStatusFlags: new List<OrderStatus>
{
	OrderStatus.Paid
});
```

#### Get Order

```csharp
var orderId = 123456789; //Must be a valid order ID.
var order = await client.GetOrderAsync(orderId);
```

#### Get Order Items

```csharp
var orderId = 1234566789; //Must be a valid order ID.
var itemsBatchList = await client.GetOrderItemsAsync(orderId);

foreach (var itemsBatch in itemsBatchList)
{
	foreach (var item in itemsBatch)
	{
		//Process item...
	}
}
```

#### Get Order Messages

```csharp
var orderId = 123456789; //Must be a valid order ID.
var messages = await client.GetOrderMessagesAsync(orderId);
```

#### Get Order Feedback

```csharp
var orderId = 123456789; //Must be a valid order ID.
var feedbacks = await client.GetOrderFeedbacksAsync(orderId);
```

#### Update Order

```csharp
var orderId = 123456789; //Must be a valid order ID.

//Note: you must only set properties which should be updated. Leave all others Null.
var updateOrder = new UpdateOrder();
updateOrder.Remarks = "Add remark";
updateOrder.IsFiled = true;
updateOrder.Cost.InsuranceCost = 2.5m;
updateOrder.Cost.Etc1 = 1.0m;
updateOrder.Shipping.TrackingNumber = "1234567892";
updateOrder.Shipping.TrackingLink = "www.foo.bar/123456789";
await client.UpdateOrder(orderId, updateOrder);
```

#### Update Order Status

```csharp
var orderId = 123456789; //Must be a valid order ID.
try
{
	//Note that the order must be outgoing in order to be able to set it to 'Shipped'.
	await client.UpdateOrderStatusAsync(orderId, OrderStatus.Shipped);
}
catch (Exception exception)
{
	//Handle invalid operation.
}
```

#### Update Payment Status

```csharp
var orderId = 123456789; //Must be a valid order ID.
try
{
	//Note that the order must be outgoing in order to be able to set it to 'Received'.
	await client.UpdatePaymentStatusAsync(orderId, PaymentStatus.Received);
}
catch (Exception exception)
{
	//Handle invalid operation.
}
```

#### Send Drive Thru

```csharp
var orderId = 123456789; //Must be a valid order ID.
try
{
	await client.SendDriveThruAsync(orderId, true);
}
catch (Exception exception)
{
	//Handle invalid operation.
}
```

### Coupons

#### Get Coupons

```csharp
var includesStatusTypes = new List<CouponStatus>
{
	CouponStatus.Open
}
var coupons = await client.GetCouponsAsync(Direction.Out, includedCouponStatusTypes: includesStatusTypes);
```

#### Get Coupon

```csharp
var couponId = 123456789; //Must be a valid coupon ID.
var coupon = await client.GetCouponAsync(couponId);
```

#### Create Coupon

```csharp
var newCoupon = new NewCoupon("bluser", "Special gift for you")
{
	DiscountType = DiscountType.Percentage,
	DiscountRate = 10
};
newCoupon.AppliesTo.ExceptOnSale = true;
newCoupon.AppliesTo.RestrictionType = CouponRestrictionType.ApplyToSpecifiedItemType;
newCoupon.AppliesTo.ItemType = ItemType.Part; //Only applies to parts.

var coupon = await client.CreateCouponAsync(newCoupon);
```

#### Update Coupon

```csharp
var updateCoupon = new UpdateCoupon
{
	DiscountType = DiscountType.Percentage,
	DiscountRate = 15 //Increase discount rate to 15 percent.
};

var couponId = 123456789; //Must be a valid coupon ID.
var coupon = await client.UpdateCouponAsync(couponId, updateCoupon);
```

#### Delete Coupon

```csharp
var couponId = 123456789; //Must be a valid coupon ID.
await client.DeleteCouponAsync(couponId);
```
