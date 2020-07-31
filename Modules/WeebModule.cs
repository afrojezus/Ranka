using BooruSharp.Booru;
using Discord;
using Discord.Commands;
using Miki.Anilist;
using Ranka.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using TenorSharp;

namespace Ranka.Modules
{
    [Name("Weeb")]
    [Summary("All weeb related commands")]
    public class WeebModule : RankaModule
    {
        [Command("booru")]
        [Summary("Get a random picture from yande.re")]
        public async Task BooruAsync
            (
                [Remainder][Summary("Search query")] string query
            )
        {
            if (query == null) throw new ArgumentNullException(nameof(query), "No input passed");

            string[] sinfulTags =
            {
                "nude",
                "nipples",
                "ass",
                "uncensored",
                "cleavage"
            };

            if (sinfulTags.Any(query.Contains))
            {
                await RankaReplyAsync("...").ConfigureAwait(false);
                return;
            }

            var booru = new Yandere();
            var result = await booru.GetRandomImageAsync(query).ConfigureAwait(false);

            if (result.tags.Where(x => sinfulTags.Any(y => x.Contains(y))).Any())
            {
                await RankaReplyAsync("..... /_ \\ It was a bit lewd, try again..").ConfigureAwait(false);
                return;
            }

            var color = await StringUtils.DiscordParseColor(new Uri(result.previewUrl.AbsoluteUri)).ConfigureAwait(false); ;

            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Booru",
                Description = $"Source: Yande.re\nTags:\n{string.Join(", ", result.tags)}",
                ImageUrl = result.previewUrl.AbsoluteUri,
                Url = result.postUrl.AbsoluteUri,
                Color = color,
            };
            eb.WithFooter(footer => { footer.Text = "Booru for Ranka"; });
            await RankaReplyAsync(eb).ConfigureAwait(false);
        }

        [Command("slap")]
        [Summary("Slap someone")]
        public async Task SlapAsync([Remainder][Summary("That someone")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            // TODO: Solve this IUser bullshit
            if (user.IsBot && user.Id == Context.Client.CurrentUser.Id)
            {
                await RankaReplyAsync("w-why? .·´¯`(>▂<)´¯`·. ").ConfigureAwait(false);
                return;
            }

            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var slaps = tenor.Search("anime slap");
            var output = slaps.GifResults[new Random().Next(slaps.GifResults.Length)];
            await RankaReplyAsync($"{user.Mention}").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("slap")]
        [Summary("Slap someone with a reason")]
        public async Task SlapAsync([Summary("That someone")] IUser user, [Remainder][Summary("Reasoning")] string reasoning)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            if (user.IsBot && user.Id == Context.Client.CurrentUser.Id)
            {
                await RankaReplyAsync($"w-why? for {reasoning}?? .·´¯`(>▂<)´¯`·.").ConfigureAwait(false);
                return;
            }
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var slaps = tenor.Search("anime slap");
            var output = slaps.GifResults[new Random().Next(slaps.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} slapped {user.Mention} for {reasoning}").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("cry")]
        [Summary("Show that you're crying")]
        public async Task CryAsync()
        {
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var cries = tenor.Search("anime cries");
            var output = cries.GifResults[new Random().Next(cries.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} is crying").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("hug")]
        [Summary("Hug yourself")]
        public async Task HugAsync()
        {
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var hugs = tenor.Search("anime hug");
            var output = hugs.GifResults[new Random().Next(hugs.GifResults.Length)];
            await RankaReplyAsync($"Oh my you got nobody? Fine I'll give you a hug ＜（＾－＾）＞").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("hug")]
        [Summary("Hug someone")]
        public async Task HugAsync([Summary("That someone")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var hugs = tenor.Search("anime hug");
            var output = hugs.GifResults[new Random().Next(hugs.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} hugs {user.Mention}").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("hug")]
        [Summary("Hug someone")]
        public async Task HugAsync([Summary("That someone")] IUser user, [Remainder][Summary("Reason")] string reason)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var hugs = tenor.Search("anime hug");
            var output = hugs.GifResults[new Random().Next(hugs.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} hugs {user.Mention} for {reason}").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("pout")]
        [Summary("Show that you're pouting")]
        public async Task PoutAsync()
        {
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var pouts = tenor.Search("anime pout");
            var output = pouts.GifResults[new Random().Next(pouts.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} is pouting").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("pout")]
        [Summary("Pout at someone")]
        public async Task PoutAsync([Summary("That someone")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            var tenor = new TenorClient(Environment.GetEnvironmentVariable("TENOR_API"));
            var pouts = tenor.Search("anime pout");
            var output = pouts.GifResults[new Random().Next(pouts.GifResults.Length)];
            await RankaReplyAsync($"{Context.User.Username} is pouting at {user.Mention}").ConfigureAwait(false);
            await RankaReplyAsync(output.Url.AbsoluteUri).ConfigureAwait(false);
        }

        [Command("anilist")]
        [Summary("Get anime/manga information from Anilist")]
        public async Task AnilistAsync([Remainder][Summary("Search query (use keyword `search:` to get multiple results)")] string query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query), "No input passed");
            AnilistClient anilist = new AnilistClient();

            if (query.ToLower().StartsWith("search:"))
            {
                query = query.Replace("search:", "");
                var results = await anilist.SearchMediaAsync(query).ConfigureAwait(false);
                EmbedBuilder _eb = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = $"SEARCH",
                        IconUrl = "https://submission-manual.anilist.co/logo.png",
                    },
                    Title = $"{results.Items.Count} results for {query}",
                    Color = Discord.Color.Blue,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Anilist for Ranka"
                    }
                };

                foreach (var item in results.Items)
                {
                    _eb.AddField(item.DefaultTitle, $"{item.Type} | [Link](https://anilist.co/{item.Type.ToString().ToLower()}/{item.Id})");
                }

                await RankaReplyAsync(_eb).ConfigureAwait(false);
                return;
            }

            var res = await anilist.GetMediaAsync(query).ConfigureAwait(false);

            var color = await StringUtils.DiscordParseColor(new Uri(res.CoverImage)).ConfigureAwait(false);

            EmbedBuilder eb = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{res.Type}",
                    IconUrl = "https://submission-manual.anilist.co/logo.png",
                },
                Title = res.DefaultTitle,
                Description = res.Description,
                Color = color,
                Url = res.Url,
                ThumbnailUrl = res.CoverImage,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Anilist for Ranka"
                }
            };

            var score = res.Score.HasValue ? $"{res.Score.Value}/100" : "N/A";
            eb.AddField("Japanese Title", res.NativeTitle, true);
            eb.AddField("Transcripted Title", res.RomajiTitle, true);
            eb.AddField("Score", score, true);
            eb.AddField("Genres", string.Join(", ", res.Genres));
            if (res.Episodes.HasValue) eb.AddField("Episodes", res.Episodes.Value, true);
            if (res.Chapters.HasValue) eb.AddField("Chapters", res.Chapters.Value, true);
            if (res.Volumes.HasValue) eb.AddField("Volumes", res.Volumes.Value, true);
            eb.AddField("Status", res.Status, true);

            await RankaReplyAsync(eb).ConfigureAwait(false);
        }
    }
}