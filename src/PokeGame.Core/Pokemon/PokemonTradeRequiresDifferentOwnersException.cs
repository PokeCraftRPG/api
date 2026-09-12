namespace PokeGame.Core.Pokemon;

public sealed class PokemonTradeRequiresDifferentOwnersException : ConflictException
{
  public PokemonTradeRequiresDifferentOwnersException(Specimen source, Specimen target)
    : base("The specified Pokémon must have different owners to be traded.")
  {
    Data["WorldId"] = source.WorldId.EntityId;
    Data["SourcePokemonId"] = source.EntityId;
    Data["TargetPokemonId"] = target.EntityId;
  }
}
