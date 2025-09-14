using Discord.Interactions;
using Moobot.Database;
using Moobot.Database.Models.Entities;
using Moobot.Database.Queries;
using Moobot.Managers;
using Moobot.Modules.Handlers;
using Moobot.Utils;
using MooBot.Configuration;
using MooBot.Database.Models.Entities;
using MooBot.Managers.CharacterAssignment;
using MooBot.Modules.Commands.Pokemon;
using MooBot.Modules.Handlers.Models.AutoAssign;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Moobot.Modules.Commands
{
    public class FunCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("moo", "Will moo for you")]
        public async Task SayMoo()
        {
            Random randomizer = new Random();
            int extraOs = randomizer.Next(1, 20);

            await RespondAsync($"Moo{new String('o', extraOs)}");
        }

        [SlashCommand("fuse", "Fuse two pokemons together")]
        public async Task FusePokemon(string pokemonHead, string pokemonBody, int variantType = -1)
        {
            pokemonHead = pokemonHead.ToLower().Trim();
            var pokemonHeadData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{pokemonHead}");
            if (pokemonHeadData == null) 
            {
                await RespondAsync($"{pokemonHead} is not a real Pokemon", ephemeral: true);
                return;
            }

            pokemonBody = pokemonBody.ToLower().Trim();
            var pokemonBodyData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{pokemonBody}");
            if (pokemonBodyData == null)
            {
                await RespondAsync($"{pokemonBody} is not a real Pokemon", ephemeral: true);
                return;
            }

            var directoriesConfig = ApplicationConfiguration.Configuration.GetSection("Directories");

            var pokemonJson = Path.Combine(directoriesConfig["BaseDirectory"], "Modules/Commands/Pokemon/PokemonIds.json");
            var fusionData = JsonConvert.DeserializeObject<PokemonList>(File.ReadAllText(pokemonJson));

            var pokemonHeadFusionData = fusionData.Pokemons.FirstOrDefault(p => p.Name.ToLower() == pokemonHeadData.Name.ToLower());
            if (pokemonHeadFusionData == null)
            {
                await RespondAsync($"{pokemonHead} does not exist is Pokemon Infinite Fusion", ephemeral: true);
                return;
            }

            var pokemonBodyFusionData = fusionData.Pokemons.FirstOrDefault(p => p.Name.ToLower() == pokemonBodyData.Name.ToLower());
            if (pokemonBodyFusionData == null)
            {
                await RespondAsync($"{pokemonBody} does not exist is Pokemon Infinite Fusion", ephemeral: true);
                return;
            }

            var spriteFilePath = "";
            var variantsAmount = 0;

            if (variantType > -1)
            {
                // 97 is the ASCII code for 'a'
                var spriteFile = $"{pokemonHeadFusionData.Id}.{pokemonBodyFusionData.Id}{(char)(variantType + 96)}.png";
                spriteFilePath = Path.Combine(directoriesConfig["Fusions"], "customs", spriteFile);
            } 
            else
            {
                var spriteFile = $"{pokemonHeadFusionData.Id}.{pokemonBodyFusionData.Id}.png";
                spriteFilePath = Path.Combine(directoriesConfig["Fusions"], "customs", spriteFile);

                if (!File.Exists(spriteFilePath))
                {
                    spriteFilePath = Path.Combine(directoriesConfig["Fusions"], pokemonHeadFusionData.Id.ToString(), spriteFile);
                }
                else
                {
                    Console.WriteLine($"{pokemonHeadFusionData.Id}.{pokemonBodyFusionData.Id}");
                    var reg = new Regex($@"{pokemonHeadFusionData.Id}\.{pokemonBodyFusionData.Id}[a-z]");
                    variantsAmount = Directory.GetFiles(Path.Combine(directoriesConfig["Fusions"], "customs"), $"{pokemonHeadFusionData.Id}.{pokemonBodyFusionData.Id}*").Count(reg.IsMatch);
                }
            }

            if (!File.Exists(spriteFilePath))
            {
                await RespondAsync($"Fusion sprite may not exist", ephemeral: true);
            }

            if (variantsAmount > 0)
            {
                await RespondWithFileAsync(spriteFilePath, text: $"This fusion has {variantsAmount} variants");
            } 
            else
            {
                await RespondWithFileAsync(spriteFilePath);
            }
            
        }

        [SlashCommand("fuse-random", "Fuse two random pokemons together")]
        public async Task FuseRandomPokemon(string basePokemon = "")
        {
            var directoriesConfig = ApplicationConfiguration.Configuration.GetSection("Directories");
            var pokemonJson = Path.Combine(directoriesConfig["BaseDirectory"], "Modules/Commands/Pokemon/PokemonIds.json");
            var fusionData = JsonConvert.DeserializeObject<PokemonList>(File.ReadAllText(pokemonJson));

            int hasToContainId = -1;

            if (basePokemon != "")
            {
                basePokemon = basePokemon.ToLower().Trim();
                var basePokemonData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{basePokemon}");
                if (basePokemon == null)
                {
                    await RespondAsync($"{basePokemon} is not a real Pokemon", ephemeral: true);
                    return;
                }

                var basePokemonFusionData = fusionData.Pokemons.FirstOrDefault(p => p.Name.ToLower() == basePokemonData.Name.ToLower());
                if (basePokemonFusionData == null)
                {
                    await RespondAsync($"{basePokemonData} does not exist is Pokemon Infinite Fusion", ephemeral: true);
                    return;
                }

                hasToContainId = basePokemonFusionData.Id;
            }

            var rand = new Random();
            var sprites = Directory.GetFiles(Path.Combine(directoriesConfig["Fusions"], "customs"), "*.png");

            if (hasToContainId != -1)
            {
                sprites = sprites
                    .Where(sprite => Path.GetFileNameWithoutExtension(sprite).StartsWith($"{hasToContainId}.") || Path.GetFileNameWithoutExtension(sprite).Contains($".{hasToContainId}"))
                    .ToArray();
            }

            var randomSprite = sprites[rand.Next(sprites.Length)];

            var pokemonIds = Path.GetFileNameWithoutExtension(randomSprite).Split('.');
            var variantType = -1;

            if (pokemonIds[1].Any(char.IsLetter))
            {
                variantType = (int)pokemonIds[1][pokemonIds[1].Length - 1] - 96; // 97 is the ASCII code for 'a'
                pokemonIds[1] = pokemonIds[1].Remove(pokemonIds[1].Length - 1, 1);
            }

            var firstPokemon = fusionData.Pokemons.FirstOrDefault(p => p.Id == int.Parse(pokemonIds[0]));
            var secondPokemon = fusionData.Pokemons.FirstOrDefault(p => p.Id == int.Parse(pokemonIds[1]));

            var fusionInfo = $"{StringUtils.Capitalize(firstPokemon.Name)} + {StringUtils.Capitalize(secondPokemon.Name)}";
            if (variantType != -1)
            {
                fusionInfo += $". Sprite variant {variantType}";
            }

            await RespondWithFileAsync(randomSprite, text: fusionInfo);
        }

        [SlashCommand("roll", "Roll a simple or complex dices")]
        public async Task Roll(string rollInput)
        {
            rollInput = rollInput.Trim();

            Random randomizer = new Random();
            if (int.TryParse(rollInput, out int number))
            {
                if (number <= 0)
                {
                    await RespondAsync("Number needs to be positive", ephemeral: true);
                    return;
                }

                await RespondAsync($"Rolled {randomizer.Next(1, number + 1)}");
                return;
            }

            var splitInput = rollInput.Split('d');
            if (!int.TryParse(splitInput[0], out int numOfDices))
            {
                await RespondAsync("Invalid amount of dices provided", ephemeral: true);
                return;
            }

            if (!int.TryParse(splitInput[1], out int numOfFaces))
            {
                await RespondAsync("Invalid amount of faces provided", ephemeral: true);
                return;
            }

            var totalResult = 0;
            var result = String.Empty;
            for (int dice = 0; dice < numOfDices; dice++)
            {
                var rollResult = randomizer.Next(1, numOfFaces + 1);
                totalResult += rollResult;
                result += rollResult + " + ";
            }

            result = result.Remove(result.Length - 3, 3);
            await RespondAsync($"{result} = {totalResult}");
        }

        [SlashCommand("animal-fact", "Picks a random fact from chosen animal")]
        public async Task GetAnimalFact(string animal = "")
        {
            animal = animal.ToLower().Trim();
            //TODO: Check if animal (or emoji) is supported else fail
            AnimalFact? animalFact = default;
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            if (animal != "")
            {
                animalFact = await dbContext.AnimalFact.GetRandomAnimalFactByAnimal(animal);
            }
            else
            {
                animalFact = await dbContext.AnimalFact.GetRandomAnimalFact();
            }

            if (animalFact == null || animalFact == default(AnimalFact))
            {
                //TODO: Button to request chosen animal? will ping on requests channel
                await RespondAsync($"I don't have any Ani**moo**l facts for {animal.ToLower()}");
                return;
            }

            var factEntry = await dbContext.AnimalFact.GetAnimalFactEntryNumber(animalFact);

            var response = $"**{animalFact.Animal} fact #{factEntry}:**";
            response += $"{Environment.NewLine}{animalFact.Fact}";

            if (animalFact.Source != null && animalFact.Source.Trim() != "")
            {
                response += $"{Environment.NewLine}-#[Source]({animalFact.Source})";
            }

            await RespondAsync(response);
        }

        [SlashCommand("media", "Picks from a list of media based on default emoji")]
        public async Task GetRandomMedia(string emoji)
        {
            emoji = emoji.ToLower().Trim();
            if (emoji == "") return;

            emoji = StringUtils.ConvertStringToUnicode(emoji);

            var dbContext = ServiceManager.GetService<DatabaseContext>();
            Emoji? emojiObj = await dbContext.Emoji.GetEmojiById(emoji);
            if (emojiObj == null || emojiObj == default(Emoji))
            {
                await RespondAsync($"No media for the emoji... yet?");
                return;
            }

            EmojiMedia? emojiMedia = await dbContext.EmojiMedia.GetRandomEmojiMediaByEmoji(emojiObj.Id);
            if (emojiMedia == null || emojiMedia == default(EmojiMedia))
            {
                await RespondAsync($"No media for the emoji... yet?");
                return;
            }

            Media? media = await dbContext.Media.GetMediaById(emojiMedia.MediaId);
            if (media == null || media == default(Media))
            {
                //It shouldn't reach this place
                await RespondAsync($"No media for the emoji... yet?");
                return;
            }

            var discordConfig = ApplicationConfiguration.Configuration.GetSection("Discord");
            var mediaChannelId = ulong.Parse(discordConfig["MediaChannelId"]);

            await RespondAsync(media.Url);
        }

        [SlashCommand("media-list", "Shows a list of supported emojis with content")]
        public async Task GetEmojisList()
        {
            var dbContext = ServiceManager.GetService<DatabaseContext>();
            List<Emoji> emojis = await dbContext.Emoji.GetAllEmojis();

            var response = $"Here's a list of supported emojis!{Environment.NewLine}";
            emojis.ForEach(e => response += StringUtils.ConvertUnicodeToString(e.Id));

            await RespondAsync(response);
        }

        [SlashCommand("whois", "Tries to find the character linked to the user in faelicapedia")]
        public async Task GetUserByCharacterName(string name)
        {
            var assignPediaConfig = ApplicationConfiguration.Configuration.GetSection("AssignPedia");
            var apiUri = new UriBuilder(assignPediaConfig["BaseApiUrl"] + "characters");

            var encodedQueryStringParams = string.Format("{0}={1}", "rosettes_key", assignPediaConfig["ApiKey"]);
            apiUri.Query = string.Join("&", encodedQueryStringParams);

            AssignedCharacters? assignedCharacters = await WebHandler.GetJsonFromApi<AssignedCharacters>(apiUri.ToString());

            if (assignedCharacters == default(AssignedCharacters)) return;

            var cleanedName = name.ToLower().Replace(" ", "");
            var nameReverse = StringUtils.ReverseWords(name).ToLower().Replace(" ", "");

            var checkCharacterMatch = new List<Func<Character?>>()
                {
                    () => assignedCharacters.Characters.FirstOrDefault(c => 
                        c.Name.ToLower().Replace(" ", "") == cleanedName),
                    () => assignedCharacters.Characters.FirstOrDefault(c =>
                        c.Name.ToLower().Replace(" ", "") == nameReverse),
                };

            Character assignedCharacter = null;

            foreach (var check in checkCharacterMatch)
            {
                var result = check();
                if (result == null) continue;

                assignedCharacter = result;
                break;
            }

            if (assignedCharacter == null)
            {
                await RespondAsync("Couldn't find the character by that name");
                return;
            }

            await RespondAsync($"It's {assignedCharacter.FaelicanName}");
        }

        [SlashCommand("whois-king", "Tries to find the user with the most assigns in the franchise in faelicapedia")]
        public async Task GetMostAssignedUserInFranchise(string name)
        {
            //TODO: This gets used in many places. Should move it to a different class
            var assignPediaConfig = ApplicationConfiguration.Configuration.GetSection("AssignPedia");
            var allFranchisesApiUrl = new UriBuilder(assignPediaConfig["BaseApiUrl"] + "franchises");

            var encodedQueryStringParams = string.Format("{0}={1}", "rosettes_key", assignPediaConfig["ApiKey"]);
            allFranchisesApiUrl.Query = string.Join("&", encodedQueryStringParams);

            Franchises? franchises = await WebHandler.GetJsonFromApi<Franchises>(allFranchisesApiUrl.ToString());

            if (franchises == default(Franchises)) return;

            var cleanedName = name.ToLower().Replace(" ", "");
            var nameReverse = StringUtils.ReverseWords(name).ToLower().Replace(" ", "");

            var checkCharacterMatch = new List<Func<Franchise?>>()
                {
                    () => franchises.Faelicans.FirstOrDefault(f =>
                        f.IpName.ToLower().Replace(" ", "") == cleanedName),
                    () => franchises.Faelicans.FirstOrDefault(f =>
                        f.IpName.ToLower().Replace(" ", "") == nameReverse),
                };

            Franchise foundFranchise = null;

            foreach (var check in checkCharacterMatch)
            {
                var result = check();
                if (result == null) continue;

                foundFranchise = result;
                break;
            }

            if (foundFranchise == null)
            {
                await RespondAsync("Couldn't find the franchise by that name");
                return;
            }

            var franchiseApiUrl = new UriBuilder(assignPediaConfig["BaseApiUrl"] + $"characters/franchise/{foundFranchise.Id}");
            franchiseApiUrl.Query = string.Join("&", encodedQueryStringParams);

            AssignedCharacters? assignedCharacters = await WebHandler.GetJsonFromApi<AssignedCharacters>(franchiseApiUrl.ToString());

            if (assignedCharacters == default(AssignedCharacters)) return;

            var mostCharactersGroup = assignedCharacters.Characters
                .GroupBy(c => c.FaelicanName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var count = mostCharactersGroup.Count();
            var mostCharacters = mostCharactersGroup?.FirstOrDefault();

            await RespondAsync($"{mostCharacters.FaelicanName} has the most with {count} out of {assignedCharacters.Characters.Count()}");
        }
    }
}