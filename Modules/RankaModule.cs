using Discord;
using Discord.Commands;
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
            await Context.Channel.SendFileAsync(file).ConfigureAwait(false);
        }

        public async Task RankaReplyAsync(EmbedBuilder emb)
        {
            if (emb == null) throw new ArgumentNullException(nameof(emb), "No EmbedBuilder passed");
            await Context.Channel.SendMessageAsync(embed: emb.Build()).ConfigureAwait(false);
        }

        public async Task RankaReplyAsync(string title, bool tts = false, EmbedBuilder emb = null)
        {
            if (emb == null)
            {
                await ReplyAsync(title, tts).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(title, tts, emb.Build()).ConfigureAwait(false);
            }
        }

        public async Task RankaFileUploadAsync(string file, EmbedBuilder embed)
        {
            if (embed == null) throw new ArgumentNullException(nameof(embed), "No EmbedBuilder passed");
            await Context.Channel.SendFileAsync(file, embed: embed.Build()).ConfigureAwait(false);
        }

        public async Task RankaActivityAsync(string s, ActivityType activity)
        {
            try
            {
                await Context.Client.SetGameAsync(s, null, activity).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task RankaDefaultActivityAsync()
        {
            try
            {
                await Context.Client.SetGameAsync(Config["default_activity"], null, ActivityType.CustomStatus).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}