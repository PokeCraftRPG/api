namespace PokeGame.Core.Pokemon;

public sealed class PokemonCannotBeTradedWithItselfException : DomainException
{
  public PokemonCannotBeTradedWithItselfException(Specimen specimen)
    : base("The specified Pokémon cannot be traded with itself.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
