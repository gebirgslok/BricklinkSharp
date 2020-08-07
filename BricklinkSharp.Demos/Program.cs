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
using System.Threading.Tasks;
using BricklinkSharp.Client;

namespace BricklinkSharp.Demos
{
    internal static class Program
    {
        static async Task Main()
        {
            BricklinkClientConfiguration.Instance.TokenValue = "2EBD09FD75BE4A3BBA9E556B297D0CAF";
            BricklinkClientConfiguration.Instance.TokenSecret = "95F3584C389340CBB20965839D1B737D";
            BricklinkClientConfiguration.Instance.ConsumerKey = "67BD8A6AD39E441D855DBDA9613DBE0B";
            BricklinkClientConfiguration.Instance.ConsumerSecret = "E36EADA87AC342D2AFD345E2769FD31A";
            //BricklinkClientConfiguration.Instance.TokenValue = "<Your Token>";
            //BricklinkClientConfiguration.Instance.TokenSecret = "<Your Token Secret>";
            //BricklinkClientConfiguration.Instance.ConsumerKey = "<Your Consumer Key>";
            //BricklinkClientConfiguration.Instance.ConsumerSecret = "<Your Consumer Secret>";

            //await CatalogDemos.GetItemImageDemo();
            //await CatalogDemos.GetItemDemo();
            //await CatalogDemos.GetSupersetsDemo();
            //await CatalogDemos.GetSubsetsDemo();
            //await CatalogDemos.GetPriceGuideDemo();
            //await CatalogDemos.GetKnownColorsDemo();

            //await ColorDemos.GetColorListDemo();
            //await ColorDemos.GetColorDemo();

            //await CategoryDemos.GetCategoryListDemo();
            //await CategoryDemos.GetCategoryDemo();

            //await InventoryDemos.CreateInventoryDemo();
            await InventoryDemos.CreateInventoriesDemo();
            await InventoryDemos.GetInventoryListDemo();

            Console.ReadKey(true);
        }
    }
}
