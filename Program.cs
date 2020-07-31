using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Pastel;
using Ranka.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ranka
{
    public class Program
    {
        private readonly IConfiguration _config;
        private DiscordSocketClient _client;

        public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public Program()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "settings.json");

            // build the configuration and assign to _config
            _config = _builder.Build();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            Console.WriteLine($"{Figgle.FiggleFonts.Slant.Render("Ranka").Pastel(Color.Green)}");
            Console.WriteLine("Maintainer: afroJ <thoralf21@gmail.com>");
            Console.WriteLine($"Version {version}");

            Console.Title = $"Ranka {version}";
        }

        public async Task MainAsync()
        {
            await CheckUpdate().ConfigureAwait(false);

            DotNetEnv.Env.Load(); // environment variables for Ranka (store confidential information in .env)

            using var services = ConfigureServices();

            _client = services.GetRequiredService<DiscordSocketClient>();

            _client.Log += Log;
            _client.Ready += ReadyAsync;
            _client.UserJoined += Client_UserJoined;
            _client.UserLeft += Client_UserLeft;
            _client.Connected += Client_Connected;
            _client.Disconnected += Client_Disconnected;

            services.GetRequiredService<CommandService>().Log += Log;

            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync().ConfigureAwait(false);
            services.GetRequiredService<ScoreService>().SetupLeaderboard();

            // Block this task until the program is closed.
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task CheckUpdate()
        {
            Console.WriteLine("Checking for updates...");

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            int versionRaw = int.Parse(version.Replace(".", ""));

            var gh = new GitHubClient(new ProductHeaderValue("Ranka"));
            var releases = await gh.Repository.Release.GetAll(281504694).ConfigureAwait(false);
            foreach (var item in releases)
            {
                int repoVersion = int.Parse(Regex.Replace(item.TagName, "[^0-9.]", "").Replace(".", ""));
                if (versionRaw < repoVersion)
                {
                    Console.WriteLine($"There's a new version of Ranka avaliable! ({item.TagName})\n".Pastel(Color.Green));
                    break;
                }
                else
                {
                    Console.WriteLine("Ranka is up to date!\n".Pastel(Color.Green));
                    break;
                }
            }
            // TODO: Updater
            /**foreach (var item in releases)
            {
                int repoVersion = int.Parse(Regex.Replace(item.TagName, "[^0-9.]", "").Replace(".", ""));
                if (versionRaw < repoVersion)
                {
                    Console.WriteLine($"There's a new version of Ranka avaliable! ({item.TagName})\nWould you like to update? (Y/N)".Pastel(Color.Green));
                    var choice = Console.ReadLine();
                    switch (choice.ToLower())
                    {
                        case "y":
                            Console.WriteLine("Starting Ranka Updater....".Pastel(Color.Gold));
                            ExecuteUpdater();
                            break;

                        default:
                            Console.WriteLine("Aborted.\n");
                            break;
                    }
                    break;
                }
                else
                {
                    Console.WriteLine("Ranka is up to date!\n".Pastel(Color.Green));
                    break;
                }
            }**/
        }

        private Task Client_Disconnected(Exception arg)
        {
            Console.WriteLine("\nRanka has disconnected from Discord");
            return Task.CompletedTask;
        }

        private Task Client_Connected()
        {
            Console.WriteLine("\nRanka is connected to Discord");
            return Task.CompletedTask;
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Bye bye {arg.Mention}").ConfigureAwait(false);
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Welcome to {arg.Guild.Name}, {arg.Mention}!").ConfigureAwait(false);
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine($"Connected as [{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}]\n");
            await _client.SetGameAsync(_config["default_activity"], null, ActivityType.Listening).ConfigureAwait(false);
            Console.WriteLine("Connected Guilds:");
            foreach (var item in _client.Guilds)
            {
                Console.WriteLine($"Active at {item.Name}:{item.Id}");
            }
        }

        private void ExecuteUpdater()
        {
            throw new NotImplementedException();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<RankaService>()
                .AddSingleton<MidoService>()
                .AddSingleton<ChromiumService>()
                .AddSingleton<ScoreService>()
                .BuildServiceProvider();
        }
    }
}