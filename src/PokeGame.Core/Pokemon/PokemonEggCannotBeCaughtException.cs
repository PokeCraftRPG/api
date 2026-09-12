namespace PokeGame.Core.Pokemon;

public sealed class PokemonEggCannotBeCaughtException : DomainException
{
  public PokemonEggCannotBeCaughtException(Specimen specimen)
    : base("A Pokémon egg cannot be caught.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EggCycles"] = specimen.EggCycles;
  }
}
