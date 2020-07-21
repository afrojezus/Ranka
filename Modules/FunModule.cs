using Discord.Commands;
using Ranka.Services;
using System;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    // I went out of ideas.
    [Name("Fun")]
    [Summary("Fun pointless commands")]
    public class FunModule : RankaModule
    {
        [Command("sex")]
        [Summary("...")]
        public async Task SexCommand()
        {
            string[] replies =
            {
                "...",
                "why.",
                "no sex!",
                "no sex!!!!",
                "I find it hard to believe why my programmer gave me a command named this. ~(>_<。)＼",
                "I said no!",
                "why, did, you, command, me, that?",
                "jesus you must be lonely.",
                "could you stop?",
                "fuck."
            };

            await RankaReplyAsync(replies[new Random().Next(replies.Length)]);
        }
    }
}