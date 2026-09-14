using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonNotInPartyException : ConflictException
{
  public PokemonNotInPartyException(Roster roster, Specimen specimen)
    : base("The specified Pokémon is not in the party.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
