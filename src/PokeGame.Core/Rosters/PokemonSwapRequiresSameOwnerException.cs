using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonSwapRequiresSameOwnerException : ConflictException
{
  public PokemonSwapRequiresSameOwnerException(Specimen source, Specimen target)
    : base("The specified Pokémon must be owned by the same trainer to be swapped.")
  {
    Data["WorldId"] = source.WorldId.EntityId;
    Data["SourcePokemonId"] = source.EntityId;
    Data["TargetPokemonId"] = target.EntityId;
  }
}
