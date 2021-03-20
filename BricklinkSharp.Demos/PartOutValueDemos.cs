using System.Collections.Generic;
using System.Threading.Tasks;
using BricklinkSharp.Client;

namespace BricklinkSharp.Demos
{
    internal static class PartOutValueDemos
    {
        public static async Task GetPartOutValueDemo()
        {
            using var client = BricklinkClientFactory.Build();
            var items = new List<KeyValuePair<string, PartOutItemType>>
            {
                new KeyValuePair<string, PartOutItemType>("6031641", PartOutItemType.Gear),
                new KeyValuePair<string, PartOutItemType>("75134", PartOutItemType.Set),
                new KeyValuePair<string, PartOutItemType>("70142", PartOutItemType.Set)
            };

            foreach (var kvp in items)
            {
                var number = kvp.Key;
                var itemType = kvp.Value;
                await client.GetPartOutValueAsync(number, breakSetsInSet: true, breakMinifigs: true, includeBox:true,
                    includeExtraParts: true, includeInstructions:true, itemType: itemType);
            }
        }
    }
}