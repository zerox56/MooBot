namespace MooBot.Modules.Commands.Pokemon
{
    public class PokemonList
    {
        public Pokemon[] Pokemons { get; set; }
    }

    public class Pokemon
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
