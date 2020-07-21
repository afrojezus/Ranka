using Discord;
using Ranka.Modules;
using System;

namespace Ranka.Services
{
    // Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Services/CustomService.cs
    public enum R_LogOutput { Console, Reply, Playing };

    public class RankaService
    {
        protected RankaModule rankaModule = null;

        public void SetRankaModule(RankaModule r)
        {
            rankaModule = r;
        }

        protected async void DiscordFileUpload(string file = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaFileUploadAsync(file);
        }

        protected async void DiscordReply(string s = null, bool tts = false, EmbedBuilder emb = null)
        {
            if (rankaModule == null) return;

            await rankaModule.RankaReplyAsync(s, tts, emb);
        }

        protected async void DiscordActivity(string s, ActivityType a)
        {
            if (rankaModule == null) return;
            await rankaModule.RankaActivityAsync(s, a);
        }

        protected void Log(string s, int output = (int)R_LogOutput.Console)
        {
            string withDate = $"{DateTime.Now:hh:mm:ss} Ranka {s}";
#if (DEBUG_VERBOSE)
            Console.WriteLine("[DEBUG] -- " + str);
#endif
            if (output == (int)R_LogOutput.Console)
            {
                Console.WriteLine("DEBUG -- " + withDate);
            }
            if (output == (int)R_LogOutput.Reply)
            {
                EmbedBuilder eb = new EmbedBuilder
                {
                    Title = s,
                    Color = Color.Green
                };

                DiscordReply(null, false, eb);
            }
            if (output == (int)R_LogOutput.Playing)
            {
                DiscordActivity(s, ActivityType.Playing);
            }
        }
    }
}