using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonAlreadyInRosterException : ConflictException
{
  public PokemonAlreadyInRosterException(Roster roster, Specimen specimen)
    : base("The specified Pokémon is already in the roster.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
