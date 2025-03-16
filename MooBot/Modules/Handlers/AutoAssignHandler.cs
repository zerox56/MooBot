using Discord;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using Moobot.Utils;
using MooBot.Configuration;
using MooBot.Database.Queries;
using MooBot.Managers.CharacterAssignment;
using MooBot.Managers.Enums;
using MooBot.Modules.Handlers.Models.AutoAssign;
using System.Text.RegularExpressions;

namespace MooBot.Modules.Handlers
{
    public class AutoAssignHandler
    {
        public static async Task AutoAssignCharacters(SocketMessage msg)
        {
            //Check if attachments, embeds or urls
            var urls = await CreateUrlsList(msg);
            Console.WriteLine("URL COUNT: " + urls.Count);
            if (urls.Count == 0) return;

            // TODO: See if multithreading helps with speed
            // TODO: Assume 1 image is posted for now. But check for multiple later
            // TODO: Reduce size of image beforehand so it can never be too large?
            var hasTooLargeImage = false;
            foreach (var url in urls)
            {
                var isValidImage = await WebHandler.CheckValidImage(url);

                if (isValidImage == WebResponseEnum.TooLarge)
                {
                    hasTooLargeImage = true;
                    continue;
                }
                if (isValidImage != WebResponseEnum.OK) continue;

                var result = await WebHandler.GetImageSauce(url);

                if (result == null) continue;

                var characterAssignments = await GetCharactersList(result);

                characterAssignments = await GetAssignedUsers(msg, characterAssignments);

                //TODO: Move this outside of the loop later
                var response = await CreateResponseMessage(characterAssignments);
                await msg.Channel.SendMessageAsync(response);
            }
        }

        private static async Task<List<CharacterAssignment>> GetCharactersList(SauceNaoSearch searchResult)
        {
            float lastSimilarity = -1;
            float highestSimilarity = -1;
            var characterAssignments = new List<CharacterAssignment>();

            foreach (var result in searchResult.Results)
            {
                if (result.Data.Characters == null) continue;
                if (lastSimilarity != -1 && Math.Abs(lastSimilarity - result.Header.GetSimilarity()) >= 10) continue;

                foreach (var character in result.Data.Characters.Split(',').ToList())
                {
                    var characterAssignment = new CharacterAssignment { Name = character.Trim(), Series = result.Data.Material };
                    characterAssignments.Add(characterAssignment);
                }

                lastSimilarity = result.Header.GetSimilarity();
                if (result.Header.GetSimilarity() > highestSimilarity)
                {
                    highestSimilarity = result.Header.GetSimilarity();
                }
            }

            // TODO: Cleanup alternatives. CharA is same as CharA (Bunny)
            characterAssignments = characterAssignments
                .GroupBy(c => new { c.Name, c.Series })
                .Select(g => g.First())
                .ToList();

            return characterAssignments;
        }

        private static async Task<List<CharacterAssignment>> GetAssignedUsers(SocketMessage msg, List<CharacterAssignment> characterAssignments)
        {
            //TODO: Move this call further up. Only have to call this once no matter the amount of images
            var assignPediaConfig = ApplicationConfiguration.Configuration.GetSection("AssignPedia");
            var apiUri = new UriBuilder(assignPediaConfig["BaseApiUrl"] + "characters");

            var encodedQueryStringParams = string.Format("{0}={1}", "rosettes_key", assignPediaConfig["ApiKey"]);
            apiUri.Query = string.Join("&", encodedQueryStringParams);

            AssignedCharacters? assignedCharacters = await WebHandler.GetJsonFromApi<AssignedCharacters>(apiUri.ToString());

            if (assignedCharacters == default(AssignedCharacters)) return null;

            var dbContext = ServiceManager.GetService<DatabaseContext>();

            foreach (var characterAssignment in characterAssignments)
            {
                //TODO: Add some potentional conversion. x series or x character name might be named slightly different on the faelicapedia.
                var cleanedupCharacter = Regex.Replace(characterAssignment.Name.ToLower(), @"\(\s*(" + characterAssignment.Series.ToLower() + @")\s*\)", "").Trim();

                Console.WriteLine(cleanedupCharacter);

                var assignedCharacter = assignedCharacters.Characters.FirstOrDefault(c => c.Name.ToLower() == cleanedupCharacter.ToLower() && 
                    c.FranchiseName.ToLower() == characterAssignment.Series.ToLower());

                if (assignedCharacter == null) continue;

                var assignedUser = await dbContext.User.GetUserById(assignedCharacter.FaelicanId, true);

                characterAssignment.User = assignedUser;
            }

            return characterAssignments;
        }

        private static async Task<string> CreateResponseMessage(List<CharacterAssignment> characterAssignments)
        {
            //TODO: Add looping through multiple images

            //If image has no assignees found, give the following response:
            //  No assignees found for: (Char), (Char), (Char)
            if (characterAssignments.All(c => c.User == null))
            {
                return $"No assignees found for: {string.Join(", ", characterAssignments.Select(c => c.Name))}";
            }

            //If image only has 1 user found who is also the uploader of an image, give the following response:
            //  Look @UserA! It's you for: (Char), (Char)
            if (characterAssignments.Select(c => c.User).Distinct().Count() <= 1)
            {
                if (characterAssignments[0].User.Id == ServiceManager.GetService<DiscordSocketClient>().CurrentUser.Id)
                {
                    return $"Look! It's Mooself for: {string.Join(", ", characterAssignments.Select(c => c.Name))}";
                }
                else
                {
                    return $"Look <@{characterAssignments[0].User.Id}>! It's you for: {string.Join(", ", characterAssignments.Select(c => c.Name))}";
                }

            }

            //If image only has 1 user and some no assignees found who is also the uploader of an image, give the following response:
            //  Look @UserA! It's you for: (Char), (Char)
            //  But no assignees found for: (Char), (Char)
            var oneAndMissingAssignments = characterAssignments.Distinct().ToList();
            if (oneAndMissingAssignments.Count == 2 && oneAndMissingAssignments.Contains(null))
            {
                var userAssignments = characterAssignments.Where(c => c.User != null).ToList();
                var oneAndMissingResponse = $"Look <@{userAssignments[0].User.Id}>! It's you for: {string.Join(", ", userAssignments.Select(c => c.Name))}";
                var noAssignments = characterAssignments.Where(c => c.User == null).ToList();
                oneAndMissingResponse += Environment.NewLine;
                oneAndMissingResponse += $"But no assignees found for: {string.Join(", ", noAssignments.Select(c => c.Name))}";
                return oneAndMissingResponse;
            }

            var response = "Tagging:";
            foreach (var characterAssignment in characterAssignments)
            {
                if (characterAssignment.User == null)
                {
                    response += $" <@{characterAssignment.User.Id}> ({characterAssignment.Name}),";
                }
                else if (characterAssignment.User.Id == ServiceManager.GetService<DiscordSocketClient>().CurrentUser.Id)
                {
                    response += $" Mooself ({characterAssignment.Name}),";
                }
                else
                {
                    response += $" No one ({characterAssignment.Name}),";
                }
            }

            response = response.Remove(response.Length - 1);
            return response;
        }


        private static async Task<List<string>> CreateUrlsList(SocketMessage msg)
        {
            var urls = new List<string>();
            var contentUrls = StringUtils.GetAllUrls(msg.Content);

            Console.WriteLine("CONTENT: " + msg.Content);
            Console.WriteLine("ATTACHMENTS: " + msg.Attachments.Any());
            Console.WriteLine("EMBEDS: " + msg.Embeds.Any());

            if (msg.Attachments.Count == 0 && msg.Embeds.Count == 0 && contentUrls.Length == 0)
            {
                return urls;
            }

            var hasSpoilers = StringUtils.CountOccurrences(msg.Content, "||") >= 2 || msg.Attachments.Any(a => a.IsSpoiler());

            urls.AddRange(msg.Attachments.Select(a => a.Url));
            urls.AddRange(msg.Embeds.Select(e => e.Url));
            urls.AddRange(contentUrls);

            return urls;
        }
    }
}