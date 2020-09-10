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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace BricklinkSharp.Client
{
    public interface IBricklinkClient
    {
        Task<CatalogItem> GetItemAsync(ItemType type, string no);

        Task<CatalogImage> GetItemImageAsync(ItemType type, string no, int colorId);

        Task<Superset[]> GetSupersetsAsync(ItemType type, string no, int colorId);

        Task<Subset[]> GetSubsetsAsync(ItemType type, string no, int colorId = 0, bool? includeOriginalBox = null,
            bool? includeInstruction = null, bool? breakMinifigs = null, bool? breakSubsets = null);

        Task<PriceGuide> GetPriceGuideAsync(ItemType type, string no, int colorId = 0, PriceGuideType? priceGuideType = null,
            Condition? condition = null, string countryCode = null, string region = null, string currencyCode = null);

        Task<KnownColor[]> GetKnownColorsAsync(ItemType type, string no);

        Task<Color[]> GetColorListAsync();

        Task<Color> GetColorAsync(int colorId);

        Task<Category[]> GetCategoryListAsync();

        Task<Category> GetCategoryAsync(int categoryId);

        Task<Inventory[]> GetInventoryListAsync(IEnumerable<ItemType> includedItemTypes = null, 
            IEnumerable<ItemType> excludedItemTypes = null,
            IEnumerable<InventoryStatusType> includedStatusFlags = null,
            IEnumerable<InventoryStatusType> excludedStatusFlags = null,
            IEnumerable<int> includedCategoryIds = null, 
            IEnumerable<int> excludedCategoryIds = null,
            IEnumerable<int> includedColorIds = null,
            IEnumerable<int> excludedColorIds = null);

        Task<Inventory> GetInventoryAsync(int inventoryId);

        Task<Inventory> CreateInventoryAsync(NewInventory newInventory);

        Task CreateInventoriesAsync(NewInventory[] newInventories);

        Task<Inventory> UpdateInventoryAsync(int inventoryId, UpdatedInventory updatedInventory);

        Task DeleteInventoryAsync(int inventoryId);

        Task<ItemMapping[]> GetElementIdAsync(string partNo, int? colorId);

        Task<ItemMapping[]> GetItemNumberAsync(string elementId);

        Task<ShippingMethod[]> GetShippingMethodListAsync();

        Task<ShippingMethod> GetShippingMethodAsync(int methodId);

        Task<Notification[]> GetNotificationsAsync();

        Task<MemberRating> GetMemberRatingAsync(string username);

        Task<Feedback[]> GetFeedbackListAsync(FeedbackDirection? direction = null);

        Task<Feedback> GetFeedbackAsync(int feedbackId);

        Task<Feedback> PostFeedbackAsync(int orderId, RatingType rating, string comment);

        Task ReplyFeedbackAsync(int feedbackId, string reply);

        Task<Order[]> GetOrdersAsync(OrderDirection direction = OrderDirection.In,
            IEnumerable<OrderStatus> includedStatusFlags = null,
            IEnumerable<OrderStatus> excludedStatusFlags = null, 
            bool filed = false);
    }
}
