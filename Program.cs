using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moobot.Managers;

namespace Moobot
{
    class Program
    {
        private DiscordSocketClient _client;

        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            DotEnv.Load();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
            services
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysFail,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            }))
            // .AddTransient<LoggerService>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionManager>())
            .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var commands = provider.GetRequiredService<InteractionService>();
            _client = provider.GetRequiredService<DiscordSocketClient>();

            await provider.GetRequiredService<InteractionManager>().InitializeAsync();

            // Subscribe to client log events
            // _client.Log += _ => provider.GetRequiredService<LoggerService>().Log(_);
            // // Subscribe to slash command log events
            // commands.Log += _ => provider.GetRequiredService<LoggerService>().Log(_);

            _client.Ready += async () =>
            {
                // If running the bot with DEBUG flag, register all commands to guild specified in config
                if (IsDebug())
                    // Id of the test guild can be provided from the Configuration object
                    await commands.RegisterCommandsToGuildAsync(UInt64.Parse(DotEnv.Read()["TEST_GUILD"]), true);
                else
                    // If not debug, register commands globally
                    await commands.RegisterCommandsGloballyAsync(true);
            };


            await _client.LoginAsync(Discord.TokenType.Bot, DotEnv.Read()["DISCORD_TOKEN"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}