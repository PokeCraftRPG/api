using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonNotInRosterException : ConflictException
{
  public PokemonNotInRosterException(Roster roster, Specimen specimen)
    : base("The specified Pokémon is not in the roster.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
