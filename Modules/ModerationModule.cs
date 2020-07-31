using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Moderation")]
    [Summary("Mod tools")]
    public class ModerationModule : RankaModule
    {
        [Command("purge")]
        [Summary("Purges last N messages in the channel")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeAsync([Summary("N number of messages")] int n)
        {
            await RankaReplyAsync($"Purging {n} messages in this channel...").ConfigureAwait(false);
            var msgs = await Context.Channel.GetMessagesAsync(n).FlattenAsync().ConfigureAwait(false);
            foreach (var item in msgs)
            {
                await item.DeleteAsync().ConfigureAwait(false);
            }
            await RankaReplyAsync("Done!").ConfigureAwait(false);
        }

        [Command("ban")]
        [Summary("Bans a menitoned user")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([Summary("User")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            await Context.Guild.AddBanAsync(user).ConfigureAwait(false);

            EmbedBuilder emb = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = $"{user.Username} banned!"
                },
                Color = Color.Red,
            };

            await RankaReplyAsync(emb).ConfigureAwait(false);
        }

        [Command("kick")]
        [Summary("Kick a menitoned user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([Summary("User")] IGuildUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");
            await user.KickAsync().ConfigureAwait(false);

            EmbedBuilder emb = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = $"{user.Username} kicked!"
                },
                Color = Color.Red,
            };

            await RankaReplyAsync(emb).ConfigureAwait(false);
        }
    }
}