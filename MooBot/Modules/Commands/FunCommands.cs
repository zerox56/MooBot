using Discord.Interactions;
using Moobot.Modules.Handlers;
using MooBot.Configuration;
using MooBot.Modules.Commands.Pokemon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

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
        public async Task FusePokemon(string firstPokemon, string secondPokemon)
        {
            firstPokemon = firstPokemon.ToLower().Trim();
            var firstPokemonData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{firstPokemon}");
            if (firstPokemonData == null) 
            {
                await RespondAsync($"{firstPokemon} is not a real Pokemon", ephemeral: true);
                return;
            }

            secondPokemon = secondPokemon.ToLower().Trim();
            var secondPokemonData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{secondPokemon}");
            if (secondPokemonData == null)
            {
                await RespondAsync($"{secondPokemon} is not a real Pokemon", ephemeral: true);
                return;
            }

            var directoriesConfig = ApplicationConfiguration.Configuration.GetSection("Directories");

            var pokemonJson = Path.Combine(directoriesConfig["BaseDirectory"], "Modules/Commands/Pokemon/PokemonIds.json");
            var fusionData = JsonConvert.DeserializeObject<PokemonList>(File.ReadAllText(pokemonJson));

            var firstPokemonFusionData = fusionData.Pokemons.FirstOrDefault(p => p.Name.ToLower() == firstPokemonData.Name.ToLower());
            if (firstPokemonFusionData == null)
            {
                await RespondAsync($"{firstPokemon} does not exist is Pokemon Infinite Fusion", ephemeral: true);
                return;
            }

            var secondPokemonFusionData = fusionData.Pokemons.FirstOrDefault(p => p.Name.ToLower() == secondPokemonData.Name.ToLower());
            if (secondPokemonFusionData == null)
            {
                await RespondAsync($"{secondPokemon} does not exist is Pokemon Infinite Fusion", ephemeral: true);
                return;
            }

            var spriteFile = $"{firstPokemonFusionData.Id}.{secondPokemonFusionData.Id}.png";

            var spriteFilePath = Path.Combine(directoriesConfig["Fusions"], "customs", spriteFile);
            if (!File.Exists(spriteFilePath))
            {
                spriteFilePath = Path.Combine(directoriesConfig["Fusions"], firstPokemonFusionData.Id.ToString(), spriteFile);
            }

            if (!File.Exists(spriteFilePath))
            {
                await RespondAsync($"Fusion sprite may not exist", ephemeral: true);
            }

            await RespondWithFileAsync(spriteFilePath);
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