using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using MooBot.Managers.Enums;
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

            await RespondAsync("Finding something spicy...");

            // Get random image
            var imageUrl = await GetRandomImage(characters, [BooruRating.Q, BooruRating.E]);
            if (imageUrl == string.Empty)
            {
                await DeleteOriginalResponseAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription(user.GlobalName)
                .WithImageUrl(imageUrl)
                .Build();
            await ModifyOriginalResponseAsync(m => {
                m.Content = "";
                m.Embed = embed;
            });
        }

        [SlashCommand("cute", "Gets a random SFW image of the assigned user")]
        public async Task CuteUser(SocketUser user)
        {
            var channel = Context.Channel;
            var guildChannel = channel as SocketTextChannel;

            if (guildChannel == null || guildChannel.IsNsfw)
            {
                await RespondAsync("Command can only be used on a SFW channel", ephemeral: true);
                return;
            }

            var characters = await GetAssignedCharacters(user.Id);
            if (characters == null) return;

            await RespondAsync("Finding something cute...");

            // Get random image
            var imageUrl = await GetRandomImage(characters, [BooruRating.G, BooruRating.S]);
            if (imageUrl == string.Empty)
            {
                await DeleteOriginalResponseAsync();
                return;
            }

            var embed = new EmbedBuilder()
                .WithDescription(user.GlobalName)
                .WithImageUrl(imageUrl)
                .Build();

            await ModifyOriginalResponseAsync(m => {
                m.Content = "";
                m.Embed = embed;
            });
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

        private static async Task<string> GetRandomImage(AssignedCharacters assignedCharacters, BooruRating[] booruRatings)
        {
            var danbooruConfig = ApplicationConfiguration.Configuration.GetSection("Boorus").GetSection("Danbooru");

            // Loop through alternative names if no results found
            // Also save this info somewhere?

            var charactersList = assignedCharacters.Characters.ToList();
            var blacklistedTags = ApplicationConfiguration.Configuration.GetSection("Boorus")["BlacklistedTags"].Split(" ");

            var failedCharactersDebug = "";

            while (charactersList.Count > 0)
            {
                var characterIndex = new Random().Next(charactersList.Count);
                Character character = charactersList[characterIndex];

                var (danbooruResults, characterTag) = await GetImageFromCharacter(character, danbooruConfig, booruRatings);

                if (danbooruResults.Count == 0)
                {
                    charactersList.RemoveAt(characterIndex);
                    failedCharactersDebug += character.Name + ", ";
                    continue;
                }

                while (danbooruResults.Count > 0)
                {
                    var index = new Random().Next(danbooruResults.Count);
                    DanbooruResult result = danbooruResults[index];

                    var tags = result.TagsGeneral.Split(" ");

                    if (!tags.Intersect(blacklistedTags).Any() && result.TagsCharacter.Contains(characterTag) &&
                        tags.Contains("animated"))
                    {
                        PostDebugMessage(failedCharactersDebug);
                        return result.FileUrl;
                    }

                    danbooruResults.RemoveAt(index);
                }

                failedCharactersDebug += character.Name + ", ";
                charactersList.RemoveAt(characterIndex);
            }

            PostDebugMessage(failedCharactersDebug);
            return string.Empty;
        }

        private static async Task<(List<DanbooruResult>, string)> GetImageFromCharacter(Character character, IConfigurationSection danbooruConfig, BooruRating[] booruRatings)
        {
            var charactersList = new List<string>();

            if (character.BooruTags != null)
            {
                var booruTagsList = character.BooruTags.Split(",");
                foreach (var booruTag in booruTagsList)
                {
                    var cleanedBooruTag = booruTag.Trim();
                    if (cleanedBooruTag == string.Empty || cleanedBooruTag.ToLower() == "none") continue;
                    charactersList.Add(booruTag.Trim());
                }
            } else {
                charactersList.Add(character.Name.Trim().Replace(" ", "_"));
            }
            
            var apiUri = new UriBuilder(danbooruConfig["BaseApiUrl"]);

            foreach (var characterName in charactersList)
            {
                var tags = "";
                if (booruRatings == null || booruRatings.Length == 0)
                {
                    tags = characterName;
                }
                else
                {
                    var ratingStrings = booruRatings.Select(r =>
                    {
                        return r switch
                        {
                            BooruRating.G => "~rating:g",
                            BooruRating.S => "~rating:s",
                            BooruRating.Q => "~rating:q",
                            BooruRating.E => "~rating:e",
                            _ => null
                        };
                    }).Where(s => s != null);

                    tags = HttpUtility.UrlEncode(string.Join(" ", ratingStrings));
                    tags += HttpUtility.UrlEncode(" " + characterName);
                }

                var queryParams = new Dictionary<string, string>() {
                    { "api_key", danbooruConfig["ApiKey"] },
                    { "login", danbooruConfig["Username"] },
                    { "tags", tags },
                    { "random", "true" }
                };
                var queryStringParams = queryParams.Select(p => string.Format("{0}={1}", p.Key, p.Value));
                apiUri.Query = string.Join("&", queryStringParams);

                List<DanbooruResult>? danbooruResults = await WebHandler.GetJsonFromApi<List<DanbooruResult>>(apiUri.ToString(), SpoofType.Danbooru);

                if (danbooruResults == default(List<DanbooruResult>) || danbooruResults.Count == 0) continue;

                return (danbooruResults, characterName);
            }

            return (new List<DanbooruResult>(), "");
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
