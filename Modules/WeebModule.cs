using Discord.Commands;
using Ranka.API;
using Ranka.Utils;
using System;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Weeb")]
    [Summary("All weeb related commands")]
    public class WeebModule : RankaModule
    {
        private enum BooruType
        {
            Danbooru,
            Yandere
        }

        [Command("booru")]
        [Summary("Get a picture from a booru site!")]
        public async Task BooruAsync
            (
                [Summary("Booru site")] string site,
                [Summary("Search query")] string query
            )
        {
            await Task.Delay(0);
            throw new NotImplementedException();
        }
    }
}