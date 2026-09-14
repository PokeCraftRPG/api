namespace PokeGame.Core.Pokemon;

public sealed class PokemonAlreadyInPartyException : ConflictException
{
  public PokemonAlreadyInPartyException(Specimen specimen)
    : base("The Pokémon is already in the party.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
