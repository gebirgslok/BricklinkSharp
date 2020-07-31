using System.Threading.Tasks;
using BricklinkSharp.Client;

namespace BricklinkSharp.Demos
{
    internal static class ColorDemos
    {
        public static async Task GetColorDemo()
        {
            var client = BricklinkClientFactory.Build();
            var color = await client.GetColorAsync(15);

            PrintHelper.PrintAsJson(color);
        }

        public static async Task GetColorListDemo()
        {
            var client = BricklinkClientFactory.Build();
            var colors = await client.GetColorListAsync();

            PrintHelper.PrintAsJson(colors);
        }
    }
}
