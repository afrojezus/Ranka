using Discord;
using Discord.Commands;
using Ranka.API;
using Ranka.Services;
using Ranka.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using WikipediaNet;
using WikipediaNet.Objects;

namespace Ranka.Modules
{
    [Name("Look Up")]
    [Summary("All look up related commands")]
    public class LookupModule : RankaModule
    {
        private readonly ChromiumService _chrome;

        public LookupModule(ChromiumService service)
        {
            _chrome = service ?? throw new ArgumentNullException(nameof(service), "No ChromiumService passed");
            _chrome.SetRankaModule(this);
        }

        [Command("steam")]
        [Summary("Looks up a Steam user using Steamazer")]
        public async Task SteamAsync([Remainder][Summary("The profile URL of a Steam user")] string query)
        {
            await RankaActivityAsync("Looking up a Steam user...", ActivityType.CustomStatus).ConfigureAwait(false);

            query = StringUtils.DiscordStringFormat(query);

            Steamazer.Steamazer steam = new Steamazer.Steamazer(Environment.GetEnvironmentVariable("STEAM_API_KEY"), Environment.GetEnvironmentVariable("STEAM_WEB_API_KEY"));

            await steam.SetID(query).ConfigureAwait(false);

            var steamData = await steam.RetrieveSteamData().ConfigureAwait(false);
            var steamWebAPIData = await steam.RetrieveSteamWebAPI(Int64.Parse(steamData.profile.steamid64)).ConfigureAwait(false);

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
                footer.Text = $"Data fetched via Steamazer ({int.Parse(steamData.auth.day_limit) - int.Parse(steamData.auth.lookups)} requests remaining for today)";
                footer.IconUrl = "https://img.pngio.com/filebreezeicons-apps-48-steamsvg-wikimedia-commons-steam-icon-png-1024_1024.png";
            });
            output = eb.Build();

            await RankaReplyAsync(eb).ConfigureAwait(false);
            await RankaDefaultActivityAsync().ConfigureAwait(false);
        }

        [Command("instagram", RunMode = RunMode.Async)]
        [Summary("Retrieves instagram search results based on a search query")]
        public async Task InstagramAsync([Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            await _chrome.NewBrowserAsync().ConfigureAwait(false);
            await RankaReplyAsync("Loading Instagram...").ConfigureAwait(false);
            await _chrome.InstaLogin().ConfigureAwait(false);
            Instafetcher instafetcher = new Instafetcher(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await instafetcher.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Done!").ConfigureAwait(false);
            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Instagram Results",
                Description = $"You requested for {query}, here's what I found!",
                Color = Color.Purple,
            };
            eb.WithFooter(footer => footer.Text = "Instafetcher for Ranka");
            int i = 1;
            foreach (var item in results.Take(10))
            {
                eb.AddField($"{i}. {item.Url.Replace("/", "")}", $"[https://www.instagram.com{item.Url}](https://www.instagram.com{item.Url})");
                i++;
            }
            await RankaReplyAsync(eb).ConfigureAwait(false);
            await instafetcher.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("instagram", RunMode = RunMode.Async)]
        [Summary("Retrieves instagram screenshot based on a search query and index")]
        public async Task InstagramAsync([Summary("Search query")] string query, [Summary("Index")] int index)
        {
            query = StringUtils.DiscordStringFormat(query);
            index -= 1;
            await _chrome.NewBrowserAsync().ConfigureAwait(false);
            await RankaReplyAsync("Loading Instagram...").ConfigureAwait(false);
            await _chrome.InstaLogin().ConfigureAwait(false);
            Instafetcher instafetcher = new Instafetcher(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await instafetcher.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await instafetcher.GetScreenshot(results, index, _chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot for user {results.ToArray()[index].Url.Replace("/", "")}").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await instafetcher.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("instagram", RunMode = RunMode.Async)]
        [Summary("Retrieves instagram screenshot based on a search query and index")]
        public async Task InstagramAsync([Summary("Index")] int index, [Summary("Screenshot height")] int height, [Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            index -= 1;
            await _chrome.NewBrowserAsync(height).ConfigureAwait(false);
            await RankaReplyAsync("Loading Instagram...").ConfigureAwait(false);
            await _chrome.InstaLogin().ConfigureAwait(false);
            Instafetcher instafetcher = new Instafetcher(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await instafetcher.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await instafetcher.GetScreenshot(results, index, _chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot for user {results.ToArray()[index].Url.Replace("/", "")}").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await instafetcher.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("instagram", RunMode = RunMode.Async)]
        [Summary("Retrieves instagram screenshot based on a search query and index")]
        public async Task InstagramAsync([Summary("Index")] int index, [Summary("Screenshot height")] int height, [Summary("Load timeout")] int timeout, [Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            index -= 1;
            await _chrome.NewBrowserAsync(height).ConfigureAwait(false);
            await RankaReplyAsync("Loading Instagram...").ConfigureAwait(false);
            await _chrome.InstaLogin().ConfigureAwait(false);
            Instafetcher instafetcher = new Instafetcher(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await instafetcher.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await instafetcher.GetScreenshot(results, index, _chrome.GetChromiumPage(), timeout).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot for user {results.ToArray()[index].Url.Replace("/", "")}").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await instafetcher.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("facebook", RunMode = RunMode.Async)]
        [Summary("Retrieves facebook search results based on a search query")]
        public async Task FacebookAsync([Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            await _chrome.NewBrowserAsync().ConfigureAwait(false);
            await RankaReplyAsync("Loading Facebook...").ConfigureAwait(false);
            await _chrome.FBLogin().ConfigureAwait(false);
            Facetracker facetracker = new Facetracker(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await facetracker.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Done!").ConfigureAwait(false);

            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Facebook results",
                Description = $"You requested for {query}, here's what I found!",
                Color = Color.Blue,
            };
            eb.WithFooter(footer => footer.Text = "Facetracker for Ranka");
            int i = 1;
            foreach (var item in results.Take(6))
            {
                eb.AddField($"{i}. {item.Title}", $"[Link to their profile]({item.Url})");
                i++;
            }
            await RankaReplyAsync(eb).ConfigureAwait(false);
            await facetracker.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("facebook", RunMode = RunMode.Async)]
        [Summary("Retrieves facebook screenshot based on a search query and index")]
        public async Task FacebookAsync([Summary("Index")] int index, [Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            index -= 1;
            await _chrome.NewBrowserAsync().ConfigureAwait(false);
            await RankaReplyAsync("Loading Facebook...").ConfigureAwait(false);
            await _chrome.FBLogin().ConfigureAwait(false);
            Facetracker facetracker = new Facetracker(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await facetracker.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await facetracker.GetScreenshot(results, index, _chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot for user {results.ToArray()[index].Title}").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await facetracker.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("facebook", RunMode = RunMode.Async)]
        [Summary("Retrieves facebook screenshot based on a search query and index")]
        public async Task FacebookAsync([Summary("Index")] int index, [Summary("Screenshot height")] int height, [Remainder][Summary("Search query")] string query)
        {
            query = StringUtils.DiscordStringFormat(query);
            index -= 1;
            await _chrome.NewBrowserAsync(height).ConfigureAwait(false);
            await RankaReplyAsync("Loading Facebook...").ConfigureAwait(false);
            await _chrome.FBLogin().ConfigureAwait(false);
            Facetracker facetracker = new Facetracker(query);
            await RankaReplyAsync("Getting search results...").ConfigureAwait(false);
            var results = await facetracker.GetSearchResults(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await facetracker.GetScreenshot(results, index, _chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot for user {results.ToArray()[index].Title}").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await facetracker.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("screenshot", RunMode = RunMode.Async)]
        [Summary("Retrieves a screenshot of a website")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ScreenshotAsync([Summary("URL (link to page)")] string url)
        {
            url = StringUtils.DiscordStringFormat(url);
            await _chrome.NewBrowserAsync().ConfigureAwait(false);
            BasicScreenshot chrome = new BasicScreenshot(url);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await chrome.GetScreenshot(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot!").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await chrome.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("screenshot", RunMode = RunMode.Async)]
        [Summary("Retrieves a screenshot of a website")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ScreenshotAsync([Summary("Screenshot height")] int height, [Summary("URL (link to page)")] string url)
        {
            url = StringUtils.DiscordStringFormat(url);
            await _chrome.NewBrowserAsync(height).ConfigureAwait(false);
            BasicScreenshot chrome = new BasicScreenshot(url);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await chrome.GetScreenshot(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot!").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await chrome.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("wiki", RunMode = RunMode.Async)]
        [Summary("Retrieves a screenshot of a wikipedia article")]
        public async Task WikiAsync([Remainder][Summary("Article name")] string article)
        {
            Wikipedia wikipedia = new Wikipedia
            {
                Limit = 5
            };
            await RankaReplyAsync($"Searching Wikipedia for {article}....").ConfigureAwait(false);
            QueryResult results = wikipedia.Search($"{article}");
            await _chrome.NewBrowserAsync(2000).ConfigureAwait(false);
            BasicScreenshot chrome = new BasicScreenshot(results.Search.ElementAt(0).Url.AbsoluteUri);
            await RankaReplyAsync("Taking screenshot...").ConfigureAwait(false);
            var file = await chrome.GetScreenshot(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await RankaReplyAsync($"Done, here's your screenshot!").ConfigureAwait(false);
            await RankaFileUploadAsync(file).ConfigureAwait(false);
            await chrome.Cleanup(_chrome.GetChromiumPage()).ConfigureAwait(false);
            await _chrome.Destroy().ConfigureAwait(false);
        }

        [Command("wikisearch", RunMode = RunMode.Async)]
        [Summary("Searches Wikipedia and fetches back results")]
        public async Task WikiSearchAsync([Remainder][Summary("Article name")] string article)
        {
            Wikipedia wikipedia = new Wikipedia
            {
                Limit = 5
            };
            await RankaReplyAsync($"Searching Wikipedia for {article}....").ConfigureAwait(false);
            QueryResult results = wikipedia.Search($"{article}");

            EmbedBuilder emb = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Wikipedia",
                    IconUrl = "https://upload.wikimedia.org/wikipedia/commons/f/ff/Wikipedia_logo_593.jpg"
                },
                Title = $"Search results for {article}",
                Color = Color.LightGrey,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Wikipedia for Ranka"
                }
            };

            foreach (var item in results.Search)
            {
                emb.AddField(item.Title, item.Url.AbsoluteUri);
            }

            await RankaReplyAsync(emb).ConfigureAwait(false);
        }
    }
}