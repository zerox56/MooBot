using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using MooBot.Modules.Handlers.Models.AutoAssign;
using MooBot.Modules.Handlers.Models.Boorus;
using System.Web;

namespace MooBot.Modules.Commands
{
    public class UserArtCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("sexualize", "Gets a random NSFW image of the assigned user")]
        public async Task SexualizeUser(SocketUser user)
        {
            var channel = Context.Channel;
            var guildChannel = channel as SocketTextChannel;

            if (guildChannel == null || !guildChannel.IsNsfw)
            {
                await RespondAsync("Command can only be used on a NSFW channel", ephemeral: true);
                return;
            }

            var characters = await GetAssignedCharacters(user.Id);
            if (characters == null) return;

            // Get random image
            var imageUrl = await GetRandomImage(characters);
            if (imageUrl == string.Empty)
            {
                await RespondAsync($"No valid images found for any of the characters of this user", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription($"{Context.User.Mention} is going to have some fun with <@{user.Id}>")
                .WithImageUrl(imageUrl)
                .Build();

            await RespondAsync(embed: embed);
        }

        private static async Task<AssignedCharacters> GetAssignedCharacters(ulong userId)
        {
            var assignPediaConfig = ApplicationConfiguration.Configuration.GetSection("AssignPedia");
            var apiUri = new UriBuilder(assignPediaConfig["BaseApiUrl"] + "characters/faelican/" + userId);

            var encodedQueryStringParams = string.Format("{0}={1}", "rosettes_key", assignPediaConfig["ApiKey"]);
            apiUri.Query = string.Join("&", encodedQueryStringParams);

            AssignedCharacters? assignedCharacters = await WebHandler.GetJsonFromApi<AssignedCharacters>(apiUri.ToString());

            if (assignedCharacters == default(AssignedCharacters)) return null;

            return assignedCharacters;
        }

        private static async Task<string> GetRandomImage(AssignedCharacters assignedCharacters)
        {
            var rule34Config = ApplicationConfiguration.Configuration.GetSection("Boorus").GetSection("Rule34");
            var apiUri = new UriBuilder(rule34Config["BaseApiUrl"]);

            // Loop through alternative names if no results found
            // Also save this info somewhere?

            var charactersList = assignedCharacters.Characters.ToList();
            var blacklistedTags = ApplicationConfiguration.Configuration.GetSection("Boorus")["BlacklistedTags"].Split(" ");

            var failedCharactersDebug = "";

            while (charactersList.Count > 0)
            {
                var characterIndex = new Random().Next(charactersList.Count);
                Character character = charactersList[characterIndex];

                var cleanedCharacter = character.Name.Trim().Replace(" ", "_");

                var queryParams = new Dictionary<string, string>() {
                    { "page", "dapi" },
                    { "s", "post" },
                    { "q", "index" },
                    { "json", "1" },
                    { "tags", $"sort:random+{cleanedCharacter}" }
                };
                var queryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, p.Value));
                apiUri.Query = string.Join("&", queryStringParams);

                List<Rule34Result>? rule34Results = await WebHandler.GetJsonFromApi<List<Rule34Result>>(apiUri.ToString());

                if (rule34Results == default(List<Rule34Result>) || rule34Results.Count == 0)
                {
                    charactersList.RemoveAt(characterIndex);
                    failedCharactersDebug += character.Name + ", ";
                    continue;
                }

                while (rule34Results.Count > 0)
                {
                    var index = new Random().Next(rule34Results.Count);
                    Rule34Result result = rule34Results[index];

                    var tags = result.Tags.Split(" ");

                    if (!tags.Intersect(blacklistedTags).Any())
                    {
                        PostDebugMessage(failedCharactersDebug);
                        return result.FileUrl;
                    }

                    rule34Results.RemoveAt(index);
                }

                failedCharactersDebug += character.Name + ", ";
                charactersList.RemoveAt(characterIndex);
            }

            PostDebugMessage(failedCharactersDebug);
            return string.Empty;
        }

        private static async void PostDebugMessage(string charactersList)
        {
            if (charactersList == string.Empty) return;

            var discordConfig = ApplicationConfiguration.Configuration.GetSection("Discord");
            var debugChannelId = ulong.Parse(discordConfig["DebugChannelId"]);

            var discordClient = ServiceManager.GetService<DiscordSocketClient>();
            var debugChannel = await discordClient.GetChannelAsync(debugChannelId) as ISocketMessageChannel;

            await debugChannel.SendMessageAsync($"Found no results for characters: {charactersList}");
        }
    }
}
