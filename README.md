# BricklinkSharp

[![NuGet](https://img.shields.io/nuget/v/BricklinkSharp?color=blue)](https://www.nuget.org/packages/BricklinkSharp/)
[![Build Status](https://dev.azure.com/jeisenbach/BricklinkSharp/_apis/build/status/gebirgslok.BricklinkSharp?branchName=master)](https://dev.azure.com/jeisenbach/BricklinkSharp/_build/latest?definitionId=1&branchName=master)

## Introduction

BricklinkSharp is a stronly-typed, easy-to-use C# client for the [bricklink](https://www.bricklink.com/v2/main.page) marketplace that gets you started with just a few lines of code. It features OAuth1 authentication, error handling and parsing of JSON data into typed instances.
It supports all .NET platforms compatible with *.NET standard 2.0* and upwards.

## Changelog

### 0.1.0
 - OAuth1 handling
 - Catalog Item
 - Color
 - Category
 
## Get started

### Demo project

Check out the [demo project](https://github.com/gebirgslok/BricklinkSharp/tree/master/BricklinkSharp.Demos) for full-featured examples.

### Prerequisites

You need to be registered on [bricklink](https://www.bricklink.com/v2/main.page) as a **seller**. Then, use this [link](https://www.bricklink.com/v2/api/register_consumer.page) to add an access token. Add your client IP address (if applicable) or *0.0.0.0* for both IP address and mask to be able to make API request from any IP.

### Install NuGet package 

#### Package Manager Console
    
	Install-Package BricklinkSharp
	
#### Command Line
    
	nuget install BricklinkSharp
	
#### Setup credentials
    
	BricklinkClientConfiguration.Instance.TokenValue = "<Your Token>";
    BricklinkClientConfiguration.Instance.TokenSecret = "<Your Token Secret>";
    BricklinkClientConfiguration.Instance.ConsumerKey = "<Your Consumer Key>";
    BricklinkClientConfiguration.Instance.ConsumerSecret = "<Your Consumer Secret>";
	
#### IBricklinkClient
    
	var client = BricklinkClientFactory.Build();
	
In applications using a IoC container you may register the *IBricklinkClient* as a service and inject it into consuming instances (e.g. controllers).
  
####  Get item
    
	var catalogItem = await client.GetItemAsync(ItemType.Part, "6089");
	
#### Get item image
    
	var catalogImage = await client.GetItemImageAsync(ItemType.OriginalBox, "1-12", 0);
	
#### Get supersets
    
	var supersets = await client.GetSupersetsAsync(ItemType.Minifig, "aqu004", 0);
	
#### Get subsets    
    
	var subsets = await client.GetSubsetsAsync(ItemType.Set, "1095-1", breakMinifigs: false);
	 
#### Get price guide
    
	var priceGuide = await client.GetPriceGuideAsync(ItemType.Part, "3003", colorId: 1, priceGuideType: PriceGuideType.Sold, condition: Condition.Used);
	
#### Get known colors
    
	var knownColors = await client.GetKnownColorsAsync(ItemType.Part, "3006");
	
#### Get color list
    
	var colors = await client.GetColorListAsync();
	
#### Get color
    
	var color = await client.GetColorAsync(15);
	
#### Get category list
    
	var categories = await client.GetCategoryListAsync();
	
#### Get category
    
	var category = await client.GetCategoryAsync(1);