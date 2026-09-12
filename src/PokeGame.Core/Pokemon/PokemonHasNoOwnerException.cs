namespace PokeGame.Core.Pokemon;

public sealed class PokemonHasNoOwnerException : DomainException
{
  public PokemonHasNoOwnerException(Specimen specimen)
    : base("The specified Pokémon does not have an owner.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
