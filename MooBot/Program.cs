using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moobot.Database;
using Moobot.Managers;
using Moobot.Modules.Commands;
using MooBot.Configuration;
using MooBot.Managers;
using MooBot.Modules.Commands.Reminders;

namespace Moobot
{
    class Program
    {
        private DiscordSocketClient _client;
        private InteractionService _commands;
        private DatabaseContext _databaseContext;
        private IConfigurationRoot _config;

        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
            .Build();

            ApplicationConfiguration.Configuration = _config;

            var databaseConfig = _config.GetSection("Database");
            var connectionString = $"server={databaseConfig["Server"]};user={databaseConfig["User"]};password={databaseConfig["Password"]};database={databaseConfig["Database"]}";
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
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMessages
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

            var discordConfig = _config.GetSection("Discord");

            _client.Ready += async () =>
            {
                // If running the bot with DEBUG flag, register all commands to guild specified in config
                if (IsDebug())
                    // Id of the test guild can be provided from the Configuration object
                    await _commands.RegisterCommandsToGuildAsync(UInt64.Parse(discordConfig["TestGuildId"]), true);
                else
                    // If not debug, register commands globally
                    await _commands.RegisterCommandsGloballyAsync(true);
            };

            _client.MessageReceived += MessageManager.OnMessageReceived;

            await _client.LoginAsync(TokenType.Bot, discordConfig["Token"]);
            await _client.StartAsync();


            await ReminderManager.InitializeReminders();

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