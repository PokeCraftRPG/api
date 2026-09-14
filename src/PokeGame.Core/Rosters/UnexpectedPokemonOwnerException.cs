using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class UnexpectedPokemonOwnerException : ConflictException
{
  public UnexpectedPokemonOwnerException(Roster roster, Specimen specimen)
  : base("The specified Pokémon should not be owned by the specified trainer.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
