using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class InvalidPokemonOwnerException : ConflictException
{
  public InvalidPokemonOwnerException(Roster roster, Specimen specimen)
    : base("The specified Pokémon is not owned by the expected trainer.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["ExpectedTrainerId"] = roster.TrainerId.EntityId;
    Data["AttemptedTrainerId"] = specimen.Ownership?.TrainerId.EntityId;
  }
}
