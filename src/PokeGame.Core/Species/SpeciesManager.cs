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
    Key? key = null;
    Number? number = null;
    Dictionary<RegionId, Number> regionalNumbers = [];
    foreach (IEvent change in species.Changes)
    {
      if (change is SpeciesCreated created)
      {
        key = created.Key;
        number = created.Number;
      }
      else if (change is SpeciesKeyChanged changed)
      {
        key = changed.Key;
      }
      else if (change is SpeciesRegionalNumberChanged regionalNumber)
      {
        regionalNumbers[regionalNumber.RegionId] = regionalNumber.Number;
      }
    }

    if (key is not null)
    {
      SpeciesId? speciesId = await _speciesQuerier.GetIdAsync(key, cancellationToken);
      if (speciesId.HasValue && !speciesId.Value.Equals(species.Id))
      {
        throw new KeyAlreadyUsedException(species, speciesId.Value.EntityId, species.Key, nameof(species.Key));
      }
    }

    if (number is not null)
    {
      SpeciesId? speciesId = await _speciesQuerier.GetIdAsync(number, cancellationToken);
      if (speciesId.HasValue && !speciesId.Value.Equals(species.Id))
      {
        throw new NumberAlreadyUsedException(species, speciesId.Value);
      }
    }

    foreach (KeyValuePair<RegionId, Number> regionalNumber in regionalNumbers)
    {
      SpeciesId? speciesId = await _speciesQuerier.GetIdAsync(regionalNumber.Key, regionalNumber.Value, cancellationToken);
      if (speciesId.HasValue && !speciesId.Value.Equals(species.Id))
      {
        throw new NumberAlreadyUsedException(species, speciesId.Value, regionalNumber.Key);
      }
    }
  }
}
