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
using System.Globalization;
using BricklinkSharp.Client.Extensions;

namespace BricklinkSharp.Client;

internal static partial class Endpoints
{
    public static Endpoint<Inventory[]> Inventories(IEnumerable<ItemType>? includedItemTypes,
        IEnumerable<ItemType>? excludedItemTypes, IEnumerable<InventoryStatus>? includedStatusFlags,
        IEnumerable<InventoryStatus>? excludedStatusFlags, IEnumerable<int>? includedCategoryIds,
        IEnumerable<int>? excludedCategoryIds, IEnumerable<int>? includedColorIds,
        IEnumerable<int>? excludedColorIds) =>
        Endpoint<Inventory[]>.Get(ApiResourceType.InventoryList, "inventories")
            .WithQuery("item_type", IncludeExclude(includedItemTypes, excludedItemTypes, t => t.ToDomainString()))
            .WithQuery("status", IncludeExclude(includedStatusFlags, excludedStatusFlags, f => f.ToDomainString()))
            .WithQuery("category_id", IncludeExclude(includedCategoryIds, excludedCategoryIds, id => id.ToString(CultureInfo.InvariantCulture)))
            .WithQuery("color_id", IncludeExclude(includedColorIds, excludedColorIds, id => id.ToString(CultureInfo.InvariantCulture)));

    public static Endpoint<Inventory> Inventory(int inventoryId) =>
        Endpoint<Inventory>.Get(ApiResourceType.Inventory, $"inventories/{inventoryId}");

    public static Endpoint<Inventory> CreateInventory(NewInventory newInventory) =>
        RejectingInvalid(() => DtoValidation.Validate(newInventory),
            () => Endpoint<Inventory>.Post(ApiResourceType.Inventory, "inventories").WithBody(newInventory));

    public static Endpoint<NoData> CreateInventories(NewInventory[] newInventories) =>
        RejectingInvalid(() => Array.ForEach(newInventories, DtoValidation.Validate),
            () => Endpoint<NoData>.Post(ApiResourceType.Inventory, "inventories").WithBody(newInventories));

    public static Endpoint<Inventory> UpdateInventory(int inventoryId, UpdateInventory updateInventory) =>
        RejectingInvalid(() => DtoValidation.Validate(updateInventory),
            () => Endpoint<Inventory>.Put(ApiResourceType.Inventory, $"inventories/{inventoryId}")
                .WithBody(updateInventory));

    public static Endpoint<NoData> DeleteInventory(int inventoryId) =>
        Endpoint<NoData>.Delete(ApiResourceType.Inventory, $"inventories/{inventoryId}");
}
