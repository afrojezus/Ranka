using Discord;
using Discord.Commands;
using Ranka.API;
using Ranka.Services;
using Ranka.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Misc")]
    [Summary("Miscellanous commands")]
    public class MiscModule : RankaModule
    {
        public ScoreService ScoreService { get; set; }

        [Command("yt", RunMode = RunMode.Async)]
        [Summary("Get a YouTube video (or song) in a downloadable format")]
        public async Task YouTube
            (
                [Summary("Download format (-v for video | -a for audio)")] string format,
                [Remainder][Summary("YouTube URL")] string url
            )
        {
            SmolYTObject output;
            SmolYT smolYT = new SmolYT(StringUtils.DiscordStringFormat(url));

            if (format == "-a")
            {
                await RankaReplyAsync("Getting audio file...").ConfigureAwait(false);
                output = smolYT.GetAudio();
            }
            else if (format == "-v")
            {
                await RankaReplyAsync("Getting video file...").ConfigureAwait(false);
                output = smolYT.GetVideo();
            }
            else
            {
                await RankaReplyAsync("Please specify a format before the link! `-a <youtube link>` for audio format, `-v <youtube link>` for video format.").ConfigureAwait(false);
                return;
            }
            await RankaReplyAsync("Done!").ConfigureAwait(false);

            EmbedBuilder eb = new EmbedBuilder
            {
                Title = output.meta.Title,
                Description = output.meta.Description,
                ThumbnailUrl = output.meta.Thumbnail,
                Url = output.meta.FileName,
                Color = Color.Red,
                Footer = new EmbedFooterBuilder
                {
                    Text = "SmolYT for Ranka"
                }
            };

            await RankaReplyAsync(eb).ConfigureAwait(false);
            await RankaFileUploadAsync(output.file).ConfigureAwait(false);

            smolYT.CleanUp();
        }

        [Command("pfp")]
        [Summary("A picture of me!")]
        public async Task PfpAsync()
        {
            EmbedBuilder eb = new EmbedBuilder
            {
                ImageUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Description = "How do I look? ehehe",
                Color = Color.Green,
            };

            await RankaReplyAsync(eb).ConfigureAwait(false);
        }

        [Command("pfp")]
        [Summary("The profile picture of another user")]
        public async Task PfpAsync([Remainder][Summary("User")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            string[] replies =
            {
                "You look good o.o",
                "Ehhh you could be better... (￣y▽,￣)╭ ",
                "Here you go!",
                "Pffft... sorry! ＞﹏＜"
            };

            EmbedBuilder eb = new EmbedBuilder
            {
                ImageUrl = user.GetAvatarUrl(),
                Description = replies[new Random().Next(replies.Length)],
                Color = Color.Green,
            };

            await RankaReplyAsync(eb).ConfigureAwait(false);
        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        [Summary("Check the leaderboard of this server")]
        public async Task LeaderboardAsync()
        {
            await Task.Run(() => ScoreService.UpdateLeaderboard(Context)).ConfigureAwait(false);

            var leaderboard = ScoreService.Leaderboard;

            ScoreObject highestScoreObject = leaderboard.Where(x => x.Score == leaderboard.Max(x => x.Score)).ElementAt(0);

            EmbedBuilder emb = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"Ranka Leaderboard for {Context.Guild.Name}",
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
                },
                Title = $"{highestScoreObject.User.Username} is leading with {highestScoreObject.Score} messages!",
                Color = Color.Green,
                ThumbnailUrl = highestScoreObject.User.GetAvatarUrl()
            };

            int i = 1;
            foreach (var item in leaderboard.Take(5))
            {
                emb.AddField($"{i}. {item.User.Username}", $"{item.Score} messages");
                i++;
            }

            await RankaReplyAsync(emb).ConfigureAwait(false);
        }
    }
}