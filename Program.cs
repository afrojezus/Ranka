using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using Ranka.Services;
using System;
using System.Diagnostics;
using System.Reflection;
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

            Console.WriteLine($"{Figgle.FiggleFonts.Slant.Render("Ranka").Pastel(Color.Green)}");

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;

            Console.Title = $"Ranka {version}";
        }

        public async Task MainAsync()
        {
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

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Client_Disconnected(Exception arg)
        {
            Console.WriteLine("Ranka has disconnected from Discord");
            return Task.CompletedTask;
        }

        private Task Client_Connected()
        {
            Console.WriteLine("Ranka is connected to Discord");
            return Task.CompletedTask;
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Bye bye {arg.Mention}");
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await arg.Guild.DefaultChannel.SendMessageAsync($"Welcome to {arg.Guild.Name}, {arg.Mention}!");
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine($"Connected as [{_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}]");
            await _client.SetGameAsync(_config["default_activity"], null, ActivityType.Listening);
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
                .BuildServiceProvider();
        }
    }
}