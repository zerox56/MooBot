﻿using Discord;
using Discord.WebSocket;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using Moobot.Utils;
using MooBot.Configuration;
using MooBot.Database.Queries;
using MooBot.Managers.CharacterAssignment;
using MooBot.Managers.Enums;
using MooBot.Modules.Handlers.Models.AutoAssign;
using OpenCvSharp.ImgHash;
using System.Text.RegularExpressions;

namespace MooBot.Modules.Handlers
{
    public class AutoAssignHandler
    {
        public static async Task AutoAssignCharacters(SocketMessage msg)
        {
            //Check if attachments, embeds or urls
            var urls = await CreateUrlsList(msg);
            if (urls.Count == 0) return;

            // TODO: See if multithreading helps with speed
            // TODO: Assume 1 image is posted for now. But check for multiple later
            // TODO: Reduce size of image beforehand so it can never be too large?
            // TODO: Get Database command status, If a limit is reached we can wait instead of making api calls first
            // TODO: Update Database only once. Maybe do an extra update if a limit is reached

            var responseMsg = await msg.Channel.SendMessageAsync("Processing imags...");

            var validUrls = new List<string>();
            var hasTooLargeImage = false;
            var invalidImage = false;
            var lastShortRemaining = 0;
            var lastLongRemaining = 0;

            foreach (var url in urls)
            {
                var isValidImage = await WebHandler.CheckValidImage(url);

                if (isValidImage == WebResponseEnum.OK)
                {
                    validUrls.Add(url);
                }
                else if (isValidImage == WebResponseEnum.TooLarge)
                {
                    hasTooLargeImage = true;
                }
                else
                {
                    invalidImage = true;
                }
            }

            if (validUrls.Count == 0) 
            {
                //responseMsg.ModifyAsync(m => m.Content = "No images found Moo can read...");
                responseMsg.DeleteAsync();
                return;
            }

            var assignedCharacters = await GetAssignedCharacters();
            if (assignedCharacters == null)
            {
                responseMsg.ModifyAsync(m => m.Content = "Something went wrong...");
                PostDebugMessage(msg, "Something went wrong", validUrls);
                return;
            }

            if (validUrls.Count >= 4)
            {
                responseMsg.ModifyAsync(m => m.Content = "More than 3 images found to check, this might take a while...");
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            CommandData shortRemainingData = await dbContext.CommandData.GetCommandDataById("saucenao_short_remaining", true);
            CommandData longRemainingData = await dbContext.CommandData.GetCommandDataById("saucenao_long_remaining", true);
            lastShortRemaining = int.Parse(shortRemainingData.Value);
            lastLongRemaining = int.Parse(longRemainingData.Value);

            var assigneesMsg = "";

            if (lastShortRemaining >= 4 && shortRemainingData.DateModified.Value.AddSeconds(30) >= DateTime.Now)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
            }

            foreach (var url in validUrls)
            {
                var result = await WebHandler.GetImageSauce(url);

                if (result == null) continue;

                lastShortRemaining = result.Header.ShortRemaining;
                lastLongRemaining = result.Header.LongRemaining;

                if (result.Results == null || result.Results.Length == 0)
                {
                    if (lastLongRemaining + validUrls.Count > 100)
                    {
                        await msg.Channel.SendMessageAsync("Moo can't process images, reached the daily timeout...");
                        return;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    result = await WebHandler.GetImageSauce(url);

                    if (result == null) continue;

                    lastShortRemaining = result.Header.ShortRemaining;
                    lastLongRemaining = result.Header.LongRemaining;
                }

                var characterAssignments = await GetCharactersList(result);
                characterAssignments = await GetAssignedUsers(assignedCharacters, characterAssignments);

                assigneesMsg += await CreateResponseMessage(characterAssignments) + Environment.NewLine;
            }

            if (StringUtils.RemoveNewLines(assigneesMsg).Trim() == "")
            {
                responseMsg.DeleteAsync();
                PostDebugMessage(msg, "Processed but no assignees found", validUrls);
                return;
            }

            responseMsg.ModifyAsync(m => m.Content = assigneesMsg);

            shortRemainingData.Value = lastShortRemaining.ToString();
            shortRemainingData.Type = "int";
            await dbContext.SaveChangesAsync();

            longRemainingData.Value = lastLongRemaining.ToString();
            longRemainingData.Type = "int";
            await dbContext.SaveChangesAsync();
        }

        private static async Task<List<CharacterAssignment>> GetCharactersList(SauceNaoSearch searchResult)
        {
            float lastSimilarity = -1;
            float highestSimilarity = -1;
            var characters = new List<CharacterAssignment>();
            var sauceNaoConfig = ApplicationConfiguration.Configuration.GetSection("SauceNao");

            foreach (var result in searchResult.Results)
            {
                if (result.Data.Characters == null) continue;
                if (result.Header.GetSimilarity() < int.Parse(sauceNaoConfig["SimilarityThreshold"])) continue;
                if (lastSimilarity != -1 && Math.Abs(lastSimilarity - result.Header.GetSimilarity()) >= 10) continue;

                foreach (var character in result.Data.Characters.Split(',').ToList())
                {
                    var characterAssignment = new CharacterAssignment { Name = character.Trim(), Series = result.Data.Material };
                    characters.Add(characterAssignment);
                }

                lastSimilarity = result.Header.GetSimilarity();
                if (result.Header.GetSimilarity() > highestSimilarity)
                {
                    highestSimilarity = result.Header.GetSimilarity();
                }
            }

            characters = characters
                .GroupBy(c => new { c.Name, c.Series })
                .Select(g => g.First())
                .ToList();

            var characterAssignments = new List<CharacterAssignment>();
            foreach (var character in characters)
            {
                if (character.Name.Contains('('))
                {
                    if (CheckCharacterSpecificsDuplicate(character.Name, characterAssignments)) continue;
                }

                if (CheckCharacterReversedDuplicate(character.Name, characterAssignments)) continue;

                characterAssignments.Add(character);
            }

            return characterAssignments;
        }

        private static bool CheckCharacterSpecificsDuplicate(string characterName, List<CharacterAssignment> characterAssignments)
        {
            var startIndex = characterName.IndexOf('(');
            var endIndex = characterName.IndexOf(')', startIndex + 1);

            if (startIndex != -1 && endIndex != -1)
            {
                characterName = characterName.Remove(startIndex, endIndex - startIndex + 1);
                return characterAssignments.Exists(c => c.Name.ToLower().Trim() == characterName);
            }

            return false;
        }

        private static bool CheckCharacterReversedDuplicate(string characterName, List<CharacterAssignment> characterAssignments)
        {
            characterName = Regex.Replace(characterName, @"\s*\(.*?\)", "").Trim();
            characterName = StringUtils.ReverseWords(characterName);
            return characterAssignments.Exists(c => c.Name.ToLower().Trim() == characterName);
        }

        private static async Task<AssignedCharacters> GetAssignedCharacters()
        {
            //TODO: Move this call further up. Only have to call this once no matter the amount of images
            var assignPediaConfig = ApplicationConfiguration.Configuration.GetSection("AssignPedia");
            var apiUri = new UriBuilder(assignPediaConfig["BaseApiUrl"] + "characters");

            var encodedQueryStringParams = string.Format("{0}={1}", "rosettes_key", assignPediaConfig["ApiKey"]);
            apiUri.Query = string.Join("&", encodedQueryStringParams);

            AssignedCharacters? assignedCharacters = await WebHandler.GetJsonFromApi<AssignedCharacters>(apiUri.ToString());

            if (assignedCharacters == default(AssignedCharacters)) return null;

            return assignedCharacters;
        }

        private static async Task<List<CharacterAssignment>> GetAssignedUsers(AssignedCharacters assignedCharacters, List<CharacterAssignment> characterAssignments)
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();

            foreach (var characterAssignment in characterAssignments)
            {
                //TODO: Add some potentional conversion. x series or x character name might be named slightly different on the faelicapedia.
                var cleanedupCharacter = Regex.Replace(characterAssignment.Name, @"\s*\(.*?\)", "").Trim();
                var cleanedupCharacterRevered = StringUtils.ReverseWords(cleanedupCharacter);

                var assignedCharacter = assignedCharacters.Characters.FirstOrDefault(c =>
                    c.Name.ToLower() == cleanedupCharacter.ToLower() ||
                    c.Name.ToLower() == cleanedupCharacterRevered.ToLower());

                if (assignedCharacter == null && !cleanedupCharacter.Contains(' '))
                {
                    //TODO: Temp fix. Check again with cleanedupCharacter and check every word
                    assignedCharacter = assignedCharacters.Characters.FirstOrDefault(c =>
                        c.Name.ToLower().StartsWith(cleanedupCharacter.ToLower()) ||
                        c.Name.ToLower().EndsWith(cleanedupCharacter.ToLower()));
                }

                if (assignedCharacter == null) continue;

                var assignedUser = await dbContext.User.GetUserById(assignedCharacter.FaelicanId, true);

                characterAssignment.User = assignedUser;
                characterAssignment.UserName = assignedCharacter.FaelicanName;
                characterAssignment.Name = StringUtils.CapitalizeAll(characterAssignment.Name);
            }

            return characterAssignments;
        }

        private static async Task<string> CreateResponseMessage(List<CharacterAssignment> characterAssignments)
        {
            //If image has no assignees found, give the following response:
            //  No assignees found for: (Char), (Char), (Char)
            if (characterAssignments.All(c => c.User == null))
            {
                //return $"No assignees found for: {string.Join(", ", characterAssignments.Select(c => c.Name))}";
                return "";
            }

            var dbContext = ServiceManager.GetService<DatabaseContext>();

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
                    User user = await dbContext.User.GetUserById(characterAssignments[0].User.Id, true);
                    var userMessagePart = "";
                    if (user.PingForAssignees)
                    {
                        userMessagePart = $"Look <@{characterAssignments[0].User.Id}>!";
                    } 
                    else
                    {
                        userMessagePart = $"Look {characterAssignments[0].UserName}!";
                    }
                    return $"{userMessagePart} It's you for: {string.Join(", ", characterAssignments.Select(c => c.Name))}";
                }

            }

            //If image only has 1 user and some no assignees found who is also the uploader of an image, give the following response:
            //  Look @UserA! It's you for: (Char), (Char)
            //  But no assignees found for: (Char), (Char)
            var oneAndMissingAssignments = characterAssignments.Distinct().ToList();
            if (oneAndMissingAssignments.Count == 2 && oneAndMissingAssignments.Any(c => c.User is null))
            {
                //TODO: Maybe split this up instead of using 2 LINQ calls.
                var userAssignments = characterAssignments.Where(c => c.User != null).ToList();
                User user = await dbContext.User.GetUserById(userAssignments[0].User.Id, true);
                var userMessagePart = "";
                if (user.PingForAssignees)
                {
                    userMessagePart = $"Look <@{userAssignments[0].User.Id}>!";
                }
                else
                {
                    userMessagePart = $"Look {userAssignments[0].UserName}!";
                }
                var oneAndMissingResponse = $"{userMessagePart} It's you for: {string.Join(", ", userAssignments.Select(c => c.Name))}";

                var noAssignments = characterAssignments.Where(c => c.User == null).ToList();
                oneAndMissingResponse += Environment.NewLine;
                oneAndMissingResponse += $"But no assignees found for: {string.Join(", ", noAssignments.Select(c => c.Name))}";
                return oneAndMissingResponse;
            }

            var response = "Tagging:";
            foreach (var characterAssignment in characterAssignments)
            {
                if (characterAssignment.User != null)
                {
                    if (characterAssignment.User.Id == ServiceManager.GetService<DiscordSocketClient>().CurrentUser.Id)
                    {
                        response += $" Mooself ({characterAssignment.Name}),";
                    }
                    else
                    {
                        User user = await dbContext.User.GetUserById(characterAssignment.User.Id, true);
                        var userMessagePart = "";
                        if (user.PingForAssignees)
                        {
                            userMessagePart = $"<@{characterAssignment.User.Id}>";
                        }
                        else
                        {
                            userMessagePart = $"{characterAssignment.UserName}";
                        }

                        response += $" {userMessagePart}({characterAssignment.Name}),";
                    }
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

        private static async void PostDebugMessage(SocketMessage msg, string debugMessage, List<string> ?validUrls)
        {
            var discordConfig = ApplicationConfiguration.Configuration.GetSection("Discord");
            var debugChannelId = ulong.Parse(discordConfig["DebugChannelId"]);

            var discordClient = ServiceManager.GetService<DiscordSocketClient>();
            var debugChannel = await discordClient.GetChannelAsync(debugChannelId) as ISocketMessageChannel;

            var guildId = (msg.Channel as SocketGuildChannel)?.Guild.Id;
            debugMessage += $"{Environment.NewLine}https://discord.com/channels/{guildId}/{msg.Channel.Id}/{msg.Id}";

            if (validUrls != null && validUrls.Count > 0)
            {
                debugMessage += Environment.NewLine + "Valid urls list: ";
                validUrls.ForEach(u => debugMessage += Environment.NewLine + "- " + u);
            }

            await debugChannel.SendMessageAsync(debugMessage);
        }
    }
}