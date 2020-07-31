using ColorThiefDotNet;
using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ranka.Utils
{
    public static class StringUtils
    {
        public static string DiscordStringFormat(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input), "No input passed");
            string output = input.Trim()
                .Replace("<", "")
                .Replace(">", "");
            return output;
        }

        public static async Task<Discord.Color> DiscordParseColor(System.Uri imageUrl)
        {
            var httpClient = new HttpClient();
            var stream = await httpClient.GetStreamAsync(imageUrl).ConfigureAwait(false);
            var bitmap = new Bitmap(stream);

            var colorThief = new ColorThief().GetColor(bitmap);

            httpClient.Dispose();
            bitmap.Dispose();

            return new Discord.Color(colorThief.Color.R, colorThief.Color.G, colorThief.Color.B);
        }
    }
}