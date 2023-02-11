using dotenv.net;
using Managers.DiscordManager;

class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private async Task MainAsync()
    {
        DotEnv.Load();

        DiscordManager discordManager = new DiscordManager();
        await discordManager.MainAsync();
    }
}