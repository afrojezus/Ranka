using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.Utils
{
    public static class RankaMessageGrabber
    {
        public static async Task<string> GetLastImage(SocketCommandContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx), "No context passed");

            var msgs = await ctx.Channel.GetMessagesAsync(50).FlattenAsync().ConfigureAwait(false);

            string img = null;

            foreach (var item in msgs)
            {
                if (item.Attachments.Count > 0)
                {
                    img = item.Attachments.ElementAt(0).Url;
                    break;
                }

                if (item.Content.StartsWith("http"))
                {
                    img = item.Content;
                    break;
                }
            }

            return img;
        }
    }
}