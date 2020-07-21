using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    [Name("Information")]
    [Summary("All information related commands")]
    public class InfoModule : RankaModule
    {
        public CommandService CommandService { get; set; }
        public IServiceProvider Provider { get; set; }

        [Command("info")]
        [Summary("Gives you technical information about Ranka")]
        public async Task InfoCommand()
        {
            EmbedBuilder eb = new EmbedBuilder();

            var framework = Assembly
                            .GetEntryAssembly()?
                            .GetCustomAttribute<TargetFrameworkAttribute>()?
                            .FrameworkName;

            var stats = new
            {
                OsPlatform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                AspDotnetVersion = framework
            };

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            eb.WithTitle("Information");
            eb.AddField("Server", Context.Guild.Name);
            eb.AddField("Creation date", Context.Guild.CreatedAt);
            eb.AddField("Users", Context.Guild.Users.Count);
            eb.AddField("Ranka Version", version, true);
            eb.AddField("Ranka Activity", Config["default_activity"], true);
            eb.AddField("Ranka Prefix", Config["prefix"], true);
            eb.AddField("Ranka Runtime", stats.AspDotnetVersion, true);
            eb.AddField("Ranka Platform", stats.OsPlatform, true);
            eb.WithColor(Color.Green);
            eb.WithThumbnailUrl(Context.Guild.IconUrl);
            eb.WithFooter(footer =>
            {
                footer.Text = "Don't think of anything weird... <( ￣^￣)>";
                footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
            });

            await RankaReplyAsync(eb);
        }

        [Command("help")]
        [Summary("This page duh!")]
        public async Task HelpAsync()
        {
            var modules = CommandService.Modules.Where(x => !string.IsNullOrWhiteSpace(x.Summary));
            EmbedBuilder eb = new EmbedBuilder();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            eb.WithTitle("Ranka Help");
            eb.WithDescription($"Do `{Config["prefix"]}help <category>` for more information on about the commands\nMy calling prefix is `{Config["prefix"]}`. Don't you forget that! ￣へ￣");
            eb.WithFooter(footer =>
            {
                footer.Text = Config["help_footer_text"] + " " + version;
                footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
            });

            eb.WithColor(Color.Green);

            foreach (var module in modules)
            {
                bool success = false;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context, Provider);
                    if (result.IsSuccess)
                    {
                        success = true;
                        break;
                    }
                }
                if (!success)
                    continue;

                eb.AddField(module.Name, module.Summary
                    + $"\n```{string.Join(", ", module.Commands.Select(o => o.Name))}```");
            }

            await RankaReplyAsync(eb);
            return;
        }

        [Command("help")]
        [Summary("This page duh!")]
        public async Task HelpAsync([Remainder][Summary("The category")] string category)
        {
            var module = CommandService.Modules.FirstOrDefault(x => x.Name.ToLower() == category.ToLower());

            if (module == null)
            {
                await RankaReplyAsync($"Mmmm... I can't find that category anywhere in me, sure you wrote it right? ←_←");
                await HelpAsync();
                return;
            }

            EmbedBuilder eb = new EmbedBuilder();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            eb.WithTitle($"{module.Name} Commands");
            eb.WithDescription($"{module.Summary}");
            eb.WithFooter(footer =>
            {
                footer.Text = Config["help_footer_text"] + " " + version;
                footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
            });

            eb.WithColor(Color.Green);

            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary)).GroupBy(x => x.Name).Select(x => x.First());
            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, Provider);
                if (result.IsSuccess)
                {
                    var name = $"{Config["prefix"]}{command.Name}";
                    var commandParameters = command.Parameters.Count > 0 ? $"\nUsage: `{string.Join(" ", command.Parameters.Select(o => $"{name} <{o.Name}>"))}`" +
                        $"\n{string.Join("\n", command.Parameters.Select(o => $"`<{o.Name}>`: {o.Summary}"))}" : null;
                    eb.AddField(name, command.Summary + commandParameters);
                }
            }

            await RankaReplyAsync(eb);
            return;
        }
    }
}