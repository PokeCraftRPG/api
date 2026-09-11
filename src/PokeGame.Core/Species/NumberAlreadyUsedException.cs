using PokeGame.Core.Regions;

namespace PokeGame.Core.Species;

public sealed class NumberAlreadyUsedException : ConflictException
{
  public NumberAlreadyUsedException(PokemonSpecies species, SpeciesId conflictId, RegionId? regionId = null)
    : base("The specified Pokémon number is already used.")
  {
    Data["WorldId"] = species.WorldId.EntityId;
    Data["SpeciesId"] = species.EntityId;
    Data["ConflictId"] = conflictId.EntityId;
    Data["RegionId"] = regionId?.EntityId;
    Data["AttemptedNumber"] = species.Number.Value;
    Data["PropertyName"] = nameof(PokemonSpecies.Number);
  }
}
