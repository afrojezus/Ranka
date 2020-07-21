using Discord;
using Discord.Commands;
using Ranka.API;
using Ranka.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Look Up")]
    [Summary("All look up related commands")]
    public class LookupModule : RankaModule
    {
        [Command("steam")]
        [Summary("Looks up a Steam user using Steamazer")]
        public async Task SteamAsync([Remainder][Summary("The profile URL of a Steam user")] string query)
        {
            await RankaActivityAsync("Looking up a Steam user...", ActivityType.CustomStatus);

            query = StringUtils.DiscordStringFormat(query);

            Steamazer.Steamazer steam = new Steamazer.Steamazer(Environment.GetEnvironmentVariable("STEAM_API_KEY"), Environment.GetEnvironmentVariable("STEAM_WEB_API_KEY"));

            await steam.SetID(query);

            var steamData = await steam.RetrieveSteamData();
            var steamWebAPIData = await steam.RetrieveSteamWebAPI(Int64.Parse(steamData.profile.steamid64));

            EmbedBuilder eb = new EmbedBuilder();
            Embed output;

            eb.WithTitle("Steam Look Up");
            eb.WithThumbnailUrl(steamData.profile.avatar);
            eb.AddField("Steam3 ID", steamData.profile.steam3, true);
            eb.AddField("Steam ID", steamData.profile.steamid, true);
            eb.AddField("Community ID", steamData.profile.steamid64, true);
            eb.AddField("Player name", steamData.profile.playername, true);
            eb.AddField("First name", steamData.profile.firstnameseen, true);
            eb.AddField("Profile bans", steamData.profile.profile_bans, true);
            eb.AddField("VAC bans", steamData.profile_status.vac, true);
            eb.AddField("Trade bans", steamData.profile_status.tradeban, true);
            eb.AddField("Community bans", steamData.profile_status.communityban, true);
            eb.AddField("Game bans", steamData.profile_status.ammount_game_bans, true);
            if (steamWebAPIData.response.players[0].realname != null)
                eb.AddField("Real name", steamWebAPIData.response.players[0].realname, true);

            if (steamData.namehistory != null)
            {
                var namehistory = "";

                foreach (var e in steamData.namehistory)
                {
                    namehistory += e.name + "\n";
                }

                eb.AddField("Name history", namehistory, true);
            }

            eb.WithColor(Color.DarkBlue);
            eb.WithFooter(footer =>
            {
                footer.Text = $"Data fetched via the .NET implementation of Steamazer ({int.Parse(steamData.auth.day_limit) - int.Parse(steamData.auth.lookups)} requests remaining for today)";
                footer.IconUrl = "https://img.pngio.com/filebreezeicons-apps-48-steamsvg-wikimedia-commons-steam-icon-png-1024_1024.png";
            });
            output = eb.Build();

            await RankaReplyAsync(eb);
            await RankaDefaultActivityAsync();
        }

        [Command("instagram", RunMode = RunMode.Async)]
        [Summary("Retrieves instagram search results based on a search query")]
        public async Task InstagramAsync([Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);

            Instafetcher instafetcher = new Instafetcher(query);
            await RankaReplyAsync("Preparing Instafetcher...");
            await instafetcher.Prepare();
            await RankaReplyAsync("Getting search results...");
            var results = await instafetcher.GetSearchResults();
            await RankaReplyAsync("Done!");
            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Instagram Results",
                Description = $"You requested for {query}, here's what I found!",
                Color = Color.Purple,
            };
            eb.WithFooter(footer => footer.Text = "Instafetcher for Ranka");
            foreach (var item in results.Take(10))
            {
                eb.AddField(item.Url.Replace("/", ""), $"[https://www.instagram.com{item.Url}](https://www.instagram.com{item.Url})");
            }
            await RankaReplyAsync(eb);
        }

        [Command("facebook", RunMode = RunMode.Async)]
        [Summary("Retrieves facebook search results based on a search query")]
        public async Task FacebookAsync([Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);

            Facetracker facetracker = new Facetracker(query);
            await RankaReplyAsync("Preparing Facetracker...");
            await facetracker.Prepare();
            await RankaReplyAsync("Getting search results...");
            var results = await facetracker.GetSearchResults();
            await RankaReplyAsync("Done!");

            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Facebook results",
                Description = $"You requested for {query}, here's what I found!",
                Color = Color.Blue,
            };
            eb.WithFooter(footer => footer.Text = "Facetracker for Ranka");
            foreach (var item in results.Take(6))
            {
                eb.AddField(item.Title, $"[Link to their profile]({item.Url})");
            }
            await RankaReplyAsync(eb);
        }
    }
}