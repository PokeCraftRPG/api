using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class UnexpectedPokemonRosterException : ConflictException
{
  public UnexpectedPokemonRosterException(Roster roster, Specimen specimen)
    : base("The specified Pokémon should not be in the specified roster.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
