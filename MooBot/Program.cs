using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moobot.Database;
using Moobot.Managers;
using Moobot.Modules.Commands;

namespace Moobot
{
    class Program
    {
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private DatabaseContext _databaseContext;

        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            DotEnv.Load();
            var envVars = DotEnv.Read();

            var connectionString = $"server={envVars["DATABASE_SERVER"]};user={envVars["DATABASE_USER"]};password={envVars["DATABASE_PASSWORD"]};database={envVars["DATABASE_DATABASE"]}";
            var serverVersion = new MySqlServerVersion(new Version(10, 10, 3));

            ServiceCollection collection = new ServiceCollection();
            collection
            .AddDbContext<DatabaseContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(connectionString, serverVersion)
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            )
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysFail,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100,
                AlwaysDownloadUsers = true
            }))
            // .AddTransient<LoggerService>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionManager>();

            ServiceManager.SetProvider(collection);

            await RunAsync();
        }

        public async Task RunAsync()
        {
            _client = ServiceManager.GetService<DiscordSocketClient>();
            _commands = ServiceManager.Provider.GetRequiredService<InteractionService>();
            _databaseContext = ServiceManager.GetService<DatabaseContext>();

            await ServiceManager.Provider.GetRequiredService<InteractionManager>().InitializeAsync();

            // Subscribe to client log events
            // _client.Log += _ => provider.GetRequiredService<LoggerService>().Log(_);
            // // Subscribe to slash command log events
            // commands.Log += _ => provider.GetRequiredService<LoggerService>().Log(_);

            _client.Ready += async () =>
            {
                // If running the bot with DEBUG flag, register all commands to guild specified in config
                if (IsDebug())
                    // Id of the test guild can be provided from the Configuration object
                    await _commands.RegisterCommandsToGuildAsync(UInt64.Parse(DotEnv.Read()["TEST_GUILD"]), true);
                else
                    // If not debug, register commands globally
                    await _commands.RegisterCommandsGloballyAsync(true);
            };

            await _client.LoginAsync(Discord.TokenType.Bot, DotEnv.Read()["DISCORD_TOKEN"]);
            await _client.StartAsync();

            await ReminderCommands.InitializeReminders();

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