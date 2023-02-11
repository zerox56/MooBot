using Discord;
using Discord.WebSocket;
using dotenv.net;

namespace Managers.DiscordManager
{
    public class DiscordManager
    {
        private readonly DiscordSocketClient Client;

        public DiscordManager()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysFail,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            });

            Client.Log += Log;
        }

        public async Task MainAsync()
        {
            await Client.LoginAsync(TokenType.Bot, DotEnv.Read()["DISCORD_TOKEN"]);
            await Client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(Timeout.Infinite);
        }

        private Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}");
            Console.ResetColor();

            return Task.CompletedTask;
        }
    }
}