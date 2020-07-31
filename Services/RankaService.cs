using Discord;
using Ranka.Modules;
using System;

namespace Ranka.Services
{
    // Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Services/CustomService.cs
    public enum LogOutput { Console, Reply, Playing };

    public class RankaService
    {
#pragma warning disable CA1051
        protected RankaModule rankaModule = null;
#pragma warning restore CA1051

        public void SetRankaModule(RankaModule r)
        {
            rankaModule = r;
        }

        protected async void DiscordFileUpload(string file = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaFileUploadAsync(file).ConfigureAwait(false);
        }

        protected async void DiscordFileUpload(string file = null, EmbedBuilder embed = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaFileUploadAsync(file, embed).ConfigureAwait(false);
        }

        protected async void DiscordReply(string s = null, bool tts = false, EmbedBuilder emb = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaReplyAsync(s, tts, emb).ConfigureAwait(false);
        }

        protected async void DiscordReply(EmbedBuilder emb = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaReplyAsync(emb).ConfigureAwait(false);
        }

        protected async void DiscordActivity(string s, ActivityType a)
        {
            if (rankaModule == null) return;
            await rankaModule.RankaActivityAsync(s, a).ConfigureAwait(false);
        }

        protected void Log(string s, int output = (int)LogOutput.Console)
        {
            string withDate = $"{DateTime.Now:hh:mm:ss} Ranka {s}";
#if (DEBUG_VERBOSE)
            Console.WriteLine("[DEBUG] -- " + str);
#endif
            if (output == (int)LogOutput.Console)
            {
                Console.WriteLine("DEBUG -- " + withDate);
            }
            if (output == (int)LogOutput.Reply)
            {
                EmbedBuilder eb = new EmbedBuilder
                {
                    Title = s,
                    Color = Color.Green
                };

                DiscordReply(null, false, eb);
            }
            if (output == (int)LogOutput.Playing)
            {
                DiscordActivity(s, ActivityType.Playing);
            }
        }
    }
}