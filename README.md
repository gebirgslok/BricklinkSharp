# BricklinkSharp

[![NuGet](https://img.shields.io/nuget/v/BricklinkSharp?color=blue)](https://www.nuget.org/packages/BricklinkSharp/)
[![Build Status](https://dev.azure.com/jeisenbach/BricklinkSharp/_apis/build/status/gebirgslok.BricklinkSharp?branchName=master)](https://dev.azure.com/jeisenbach/BricklinkSharp/_build/latest?definitionId=1&branchName=master)

## Introduction

BricklinkSharp is a stronly-typed, easy-to-use C# client for the [bricklink](https://www.bricklink.com/v2/main.page) marketplace that gets you started with just a few lines of code. It features OAuth1 authentication, error handling and parsing of JSON data into typed instances.
It supports all .NET platforms compatible with *.NET standard 2.0* and upwards.

## Changelog

### 0.4.0
 - Order
 - *IBricklinkClient* implements *IDisposable* and manages one instance of *HttpClient*

### 0.3.0
 - Feedback
 - Member / Get member rating
 - Push Notification
 - Setting / Shipping methods
 - Renamed *IBricklinkClient.GetItemNumber* to *IBricklinkClient.GetItemNumberAsync* (**breaking**)
 - Renamed *IBricklinkClient.GetElementId* to *IBricklinkClient.GetElementIdAsync* (**breaking**)

### 0.2.0
 - User Inventory
 - Item Mapping

### 0.1.0
 - OAuth1 handling
 - Catalog Item
 - Color
 - Category
 
## Get started

### Demo project

Check out the [demo project](https://github.com/gebirgslok/BricklinkSharp/tree/master/BricklinkSharp.Demos) for full-featured examples.

### Prerequisites

You need to be registered on [bricklink](https://www.bricklink.com/v2/main.page) as a **seller**. Then, use this [link](https://www.bricklink.com/v2/api/register_consumer.page) to add an access token. Add your client IP address (if applicable) or *0.0.0.0* for both IP address and mask to be able to make API requests from any IP.

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
It's recommended to create and use one *IBricklinkClient* client throughout  the lifetime of your application.

In applications using an IoC container you may register the *IBricklinkClient* as a service and inject it into consuming instances (e.g. controllers).
See the below examples to register the *IBricklinkClient* as single instance (Singleton).
	
#### [Autofac](https://autofac.org/) example
```csharp
containerBuilder.Register(c => BricklinkClientFactory.Build())
	.As<IBricklinkClient>()
	.SingleInstance();
```

#### [Microsoft.Extensions.DependencyInjection] example
```csharp
services.AddSingleton(typeof(IBricklinkClient), provider =>
{
    return BricklinkClientFactory.Build();
});  
```

### Item Catalog

####  Get item
```csharp 
var catalogItem = await client.GetItemAsync(ItemType.Part, "6089");
```	
#### Get item image
```csharp  
var catalogImage = await client.GetItemImageAsync(ItemType.OriginalBox, "1-12", 0);
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
var priceGuide = await client.GetPriceGuideAsync(ItemType.Part, "3003", colorId: 1, priceGuideType: PriceGuideType.Sold, condition: Condition.Used);
```	
#### Get known colors
```csharp
var knownColors = await client.GetKnownColorsAsync(ItemType.Part, "3006");
```
### Color

#### Get color list
```csharp    
var colors = await client.GetColorListAsync();
```
#### Get color
```csharp    
var color = await client.GetColorAsync(15);
```
### Category

#### Get category list
```csharp
var categories = await client.GetCategoryListAsync();
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
var excludedStatusFlags = new List<InventoryStatusType> { Unavailable };
var inventories = await client.GetInventoryListAsync(includedTypes, excludedStatusFlags: excludedStatusFlags);
```
#### Create inventory
```csharp
var newInventory = new NewInventory
{
	ColorId = 1,
	Condition = Condition.Used,
	Item = new ItemBase
	{
		Number = "3003",
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
var inventoryList = await client.GetInventoryListAsync();
var id = inventoryList.First().InventoryId;
var updatedInventory = new UpdatedInventory { ChangedQuantity = 21, Remarks = "Remarks added." };
var inventory = await client.UpdatedInventoryAsync(id, updatedInventory);
```
#### Delete inventory
```csharp	
var inventoryList = await client.GetInventoryListAsync();
var id = inventoryList.First().InventoryId;
await client.DeleteInventoryAsync(id);
```

### Item Mapping

#### Get ElementID
The method returns an array of *ItemMapping* objects. If a color ID is specified the array will contain just one element. Otherwise the array will contain mappings for every available color.
```csharp
var itemMappings = await client.GetElementIdAsync("3003", 1);
```	
#### Get item number 
```csharp
var itemMapping = await client.GetItemNumberAsync("300301");
```

### Push Notification

#### Get notifications
```csharp
var notifications = await client.GetNotificationsAsync();
```

### Setting

#### Get shipping method list
```csharp
var shippingMethods = await client.GetShippingMethodListAsync();
```
#### Get shipping method
```csharp
var shippingMethod = await client.GetShippingMethodAsync(123);
```

### Member

#### Get member rating
```csharp
var rating = await client.GetMemberRatingAsync("bluser");
```

### Feedback

#### Get feedback list
```csharp
var feedbackInList = await client.GetFeedbackListAsync(FeedbackDirection.In);
var feedbackOutList = await client.GetFeedbackListAsync(FeedbackDirection.Out);
var feedbackListAll = await client.GetFeedbackListAsync();
```

#### Get feedback
```csharp
var feedbackListAll = await client.GetFeedbackListAsync()
var id = feedbackListAll.First().FeedbackId;
var feedback = await client.GetFeedbackAsync(id);
```

#### Post feedback
```csharp
var orderId = 1234567; //Must be a valid order ID.
var feedback = await client.PostFeedbackAsync(orderId, RatingType.Praise, "Awesome transaction, praise!");
```

#### Reply feedback
```csharp
var feedbackId = 1234567; //Must be a valid feedback ID.
await client.ReplyFeedbackAsync(feedbackId, "Thank you for your kind comment.");
```

### Order

#### Get Orders
```csharp
//Exclude all purged orders.
var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
{
	OrderStatus.Purged
});
```
```csharp
//Only paid orders that must be shipped.
var orders = await client.GetOrdersAsync(OrderDirection.Out, excludedStatusFlags: new List<OrderStatus>
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
var feedbacks = await client.GetOrderFeedbackAsync(orderId);
```

#### Update Order
```csharp
var orderId = 123456789; //Must be a valid order ID.

//Note: you must only set properties which should be updated. Leave all others Null.
var updateOrder = new UpdateOrder();
updateOrder.Remarks = "Add remark";
updateOrder.IsFiled = true;
updateOrder.Cost.Insurance = 2.5m;
updateOrder.Cost.Etc1 = 1.0m;
updateOrder.Shipping.TrackingNo = "1234567892";
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
