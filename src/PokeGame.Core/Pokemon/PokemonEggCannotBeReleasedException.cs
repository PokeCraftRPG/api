namespace PokeGame.Core.Pokemon;

public sealed class PokemonEggCannotBeReleasedException : DomainException
{
  public PokemonEggCannotBeReleasedException(Specimen specimen)
    : base("A Pokémon egg cannot be released.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EggCycles"] = specimen.EggCycles;
  }
}
