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
    internal static class InventoryDemos
    {
        public static async Task UpdatedInventoryDemo(int id)
        {
            using var client = BricklinkClientFactory.Build();
            var inventory = await client.UpdateInventoryAsync(id, new UpdatedInventory { ChangedQuantity = 21, Remarks = "Remarks added." });
            
            PrintHelper.PrintAsJson(inventory);
        }

        public static async Task DeleteInventoryDemo(int id)
        {
            using var client = BricklinkClientFactory.Build();
            await client.DeleteInventoryAsync(id);
            Console.WriteLine($"Successfully deleted inventory with ID = {id}.");
        }

        public static async Task CreateInventoriesDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var newInventories = new NewInventory[]
            {
                new NewInventory
                {
                    ColorId = 2,
                    Condition = Condition.Used,
                    Item = new ItemBase
                    {
                        Number = "3003",
                        Type = ItemType.Part
                    },
                    Quantity = 5,
                    UnitPrice = 0.01M,
                    Remarks = "Good used condition"
                },

                new NewInventory
                {
                    ColorId = 3,
                    Condition = Condition.Used,
                    Item = new ItemBase
                    {
                        Number = "3003",
                        Type = ItemType.Part
                    },
                    Quantity = 5,
                    UnitPrice = 0.01M,
                    Remarks = "Good used condition"
                }
            };

            await client.CreateInventoriesAsync(newInventories);
        }

        public static async Task<Inventory> CreateInventoryDemo()
        {
            using var client = BricklinkClientFactory.Build();
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
            PrintHelper.PrintAsJson(inventory);
            return inventory;
        }

        public static async Task GetInventoryDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var inventory = await client.GetInventoryAsync(1);

            PrintHelper.PrintAsJson(inventory);
        }

        public static async Task GetInventoryListDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var inventories = await client.GetInventoryListAsync(new List<ItemType> { ItemType.Part, ItemType.Minifig },
                excludedStatusFlags: new List<InventoryStatusType> { InventoryStatusType.Reserved });

            PrintHelper.PrintAsJson(inventories);
        }
    }
}
