using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class InvalidRosterSwapException : ConflictException
{
  public InvalidRosterSwapException(Roster roster, Specimen source, Specimen target)
    : base("Exactly one of the specified Pokémon must be in the party.")
  {
    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["SourcePokemonId"] = source.EntityId;
    Data["TargetPokemonId"] = target.EntityId;
  }
}
