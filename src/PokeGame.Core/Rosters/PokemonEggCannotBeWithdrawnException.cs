using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters;

public sealed class PokemonEggCannotBeWithdrawnException : DomainException
{
  public PokemonEggCannotBeWithdrawnException(Roster roster, Specimen specimen)
    : base("A Pokémon egg cannot be withdrawn.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, specimen, nameof(specimen));

    Data["WorldId"] = roster.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = roster.TrainerId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EggCycles"] = specimen.EggCycles;
  }
}
