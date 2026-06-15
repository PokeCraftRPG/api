using Logitar.EventSourcing;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;

namespace PokeGame.Core.Species;

public interface ISpeciesManager
{
  Task EnsureUnicityAsync(PokemonSpecies species, CancellationToken cancellationToken = default);
}

internal class SpeciesManager : ISpeciesManager
{
  private readonly ISpeciesQuerier _speciesQuerier;

  public SpeciesManager(ISpeciesQuerier speciesQuerier)
  {
    _speciesQuerier = speciesQuerier;
  }

  public async Task EnsureUnicityAsync(PokemonSpecies species, CancellationToken cancellationToken)
  {
    Number? number = null;
    Slug? key = null;
    Dictionary<RegionId, Number> regionalNumbers = [];

    foreach (IEvent change in species.Changes)
    {
      if (change is SpeciesCreated)
      {
        number = species.Number;
      }
      else if (change is SpeciesKeyChanged)
      {
        key = species.Key;
      }
      else if (change is SpeciesRegionalNumberChanged regionalNumber)
      {
        regionalNumbers[regionalNumber.RegionId] = regionalNumber.Number;
      }
    }

    if (number is not null)
    {
      // TODO(fpion): implement
    }

    if (key is not null)
    {
      SpeciesId? otherId = await _speciesQuerier.TryGetIdAsync(key, cancellationToken);
      if (otherId.HasValue && !otherId.Value.Equals(species.Id))
      {
        throw new KeyAlreadyUsedException(species, otherId.Value.EntityId, key, nameof(species.Key));
      }
    }

    foreach (KeyValuePair<RegionId, Number> regionalNumber in regionalNumbers)
    {
      // TODO(fpion): implement
    }
  }
}
