namespace PokeGame.Core.Pokemon;

public sealed class PokemonEggCannotBeWithdrawnException : DomainException
{
  public PokemonEggCannotBeWithdrawnException(Specimen specimen)
    : base("A Pokémon egg cannot be withdrawn.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EggCycles"] = specimen.EggCycles;
  }
}
