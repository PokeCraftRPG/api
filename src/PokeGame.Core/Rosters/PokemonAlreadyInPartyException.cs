using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonAlreadyInPartyException : ConflictException
{
  public PokemonAlreadyInPartyException(Roster roster, Specimen specimen)
    : base("The specified Pokémon is already in the party.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
  }
}
