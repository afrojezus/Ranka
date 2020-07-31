using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Ranka.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Ranka.Services
{
    internal class CommandHandler
    {
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider s)
        {
            _config = s.GetRequiredService<IConfiguration>();
            _commands = s.GetRequiredService<CommandService>();
            _client = s.GetRequiredService<DiscordSocketClient>();
            _services = s;

            _commands.CommandExecuted += CommandExecutedAsync;

            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task InstallCommandsAsync()
        {
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: _services).ConfigureAwait(false);
        }

        // For fun.
        private async Task NonPrefixMessageHandler(SocketCommandContext ctx, int argPos)
        {
            if (ctx.Message.Author.IsBot || ctx.Message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var rootdir = AppDomain.CurrentDomain.BaseDirectory;
            RankaJSON.Responses responses;

            if (!File.Exists($"{rootdir}/responses.{ctx.Guild.Id}.json"))
            {
                // Load the default responses file
                responses = JsonConvert.DeserializeObject<RankaJSON.Responses>(File.ReadAllText($"{rootdir}/responses.json"));
            }
            else
            {
                // There's a specific responses file for this server
                responses = JsonConvert.DeserializeObject<RankaJSON.Responses>(File.ReadAllText($"{rootdir}/responses.{ctx.Guild.Id}.json"));
            }

            foreach (var response in responses.CommunicationableMessages)
            {
                if (ctx.Message.Content.ToLower().Contains(response.Trigger))
                {
                    if (response.Response.EndsWith(".JPEG"))
                        await ctx.Channel.SendFileAsync(response.Response).ConfigureAwait(false);
                    else
                        await ctx.Channel.SendMessageAsync(response.Response).ConfigureAwait(false);

                    return;
                }
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message)) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            if (!messageParam.Author.IsBot)
            {
                var nonpfxctx = new SocketCommandContext(_client, message);
                await MessageLogger(nonpfxctx, argPos).ConfigureAwait(false);
            }

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(Char.Parse(_config["prefix"]), ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
            {
                var nonpfxctx = new SocketCommandContext(_client, message);
                await NonPrefixMessageHandler(nonpfxctx, argPos).ConfigureAwait(false);
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services).ConfigureAwait(false);
        }

        private async Task MessageLogger(SocketCommandContext ctx, int argPos)
        {
            var rootdir = AppDomain.CurrentDomain.BaseDirectory;

            var logString = $"[{ctx.Message.Timestamp.UtcDateTime}][{ctx.Message.Author.Username}#{ctx.Message.Author.Discriminator}|{ctx.Message.Author.Id}] {ctx.Message.Content} [{ctx.Message.Channel.Name} ({ctx.Message.Channel.Id})][{ctx.Guild.Name} ({ctx.Guild.Id})]\n";

            await File.AppendAllTextAsync($"{rootdir}log.{ctx.Guild.Id}.txt", logString).ConfigureAwait(false);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method
            if (!command.IsSpecified)
            {
                Console.WriteLine($"Command failed to execute for [{context.User.Username + context.User.Discriminator}]!");
                return;
            }

            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                Console.WriteLine($"Command {command.Value.Name} executed for -> [{context.User.Username + context.User.Discriminator}]");
                return;
            }

            // failure scenario, let's let the user know
            await context.Channel.SendMessageAsync($"❌ **An error occurred!**\n{result.ErrorReason}").ConfigureAwait(false);
        }
    }
}