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
            firstPokemon = firstPokemon.Trim();
            var firstPokemonData = await WebHandler.GetPokemonJson($"https://pokeapi.co/api/v2/pokemon/{firstPokemon}");
            if (firstPokemonData == null) 
            {
                await RespondAsync($"{firstPokemon} is not a real Pokemon", ephemeral: true);
                return;
            }

            secondPokemon = secondPokemon.Trim();
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
            var spriteFilePath = Path.Combine(directoriesConfig["Fusions"], firstPokemonFusionData.Id.ToString(), spriteFile);
            if (!File.Exists(spriteFilePath))
            {
                await RespondAsync($"Fusion sprite may not exist", ephemeral: true);
            }

            await RespondWithFileAsync(spriteFilePath);
        }
    }
}