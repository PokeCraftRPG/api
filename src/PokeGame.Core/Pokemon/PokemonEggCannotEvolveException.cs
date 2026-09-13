namespace PokeGame.Core.Pokemon;

public sealed class PokemonEggCannotEvolveException : DomainException
{
  public PokemonEggCannotEvolveException(Specimen specimen)
    : base("A Pokémon egg cannot evolve.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EggCycles"] = specimen.EggCycles;
  }
}
