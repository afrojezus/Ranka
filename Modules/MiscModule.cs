using Discord.Commands;
using Ranka.API;
using Ranka.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Misc")]
    [Summary("Miscellanous commands")]
    public class MiscModule : RankaModule
    {
        [Command("yt", RunMode = RunMode.Async)]
        [Summary("Get a YouTube video (or song) in a downloadable format")]
        public async Task YouTube
            (
                [Summary("YouTube URL")] string url,
                [Summary("Download format (-v for video | -a for audio)")] string format = null
            )
        {
            string output;
            SmolYT smolYT = new SmolYT(StringUtils.DiscordStringFormat(url));

            if (format == "-a")
            {
                output = await smolYT.GetAudio();
            }
            else if (format == "-v")
            {
                output = await smolYT.GetVideo();
            }
            else
            {
                await Context.Channel.SendMessageAsync("Please specify a format after the link! `<youtube link> -a` for audio format, `<youtube link> -v` for video format.");
                return;
            }

            await Context.Channel.SendFileAsync(output);

            smolYT.CleanUp();
        }
    }
}