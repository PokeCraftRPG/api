namespace PokeGame.Core.Pokemon;

public sealed class PokemonNotInPartyException : ConflictException
{
  public PokemonNotInPartyException(Specimen specimen)
    : base("The Pokémon is not in the party.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
