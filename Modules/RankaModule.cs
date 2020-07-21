using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    // Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Modules/CustomModule.cs
    public class RankaModule : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Config { get; set; }

        public async Task RankaFileUploadAsync(string file)
        {
            await Context.Channel.SendFileAsync(file);
        }

        public async Task RankaReplyAsync(EmbedBuilder emb)
        {
            await Context.Channel.SendMessageAsync(embed: emb.Build());
        }

        public async Task RankaReplyAsync(string title, bool tts = false, EmbedBuilder emb = null)
        {
            if (emb == null)
            {
                await ReplyAsync(title, tts);
            }
            else
            {
                await ReplyAsync(title, tts, emb.Build());
            }
        }

        public async Task RankaActivityAsync(string s, ActivityType activity)
        {
            try
            {
                await (Context.Client as DiscordSocketClient).SetGameAsync(s, null, activity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task RankaDefaultActivityAsync()
        {
            try
            {
                await (Context.Client as DiscordSocketClient).SetGameAsync(Config["default_activity"], null, ActivityType.CustomStatus);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}