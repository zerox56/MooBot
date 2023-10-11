using Discord.Commands;
using Discord.Interactions;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using MooBot.Modules.Commands.Pokemon;
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
    }
}