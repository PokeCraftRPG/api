using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public interface ISpeciesManager
{
  Task<SpeciesAggregate> FindAsync(string species, string propertyName, CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<RegionId, Number?>> FindRegionalNumbersAsync(
    IEnumerable<RegionalNumberPayload> payloads,
    string propertyName,
    CancellationToken cancellationToken = default);
}

internal class SpeciesManager : ISpeciesManager
{
  private readonly IContext _context;
  private readonly IRegionQuerier _regionQuerier;
  private readonly ISpeciesQuerier _speciesQuerier;
  private readonly ISpeciesRepository _speciesRepository;

  public SpeciesManager(IContext context, IRegionQuerier regionQuerier, ISpeciesQuerier speciesQuerier, ISpeciesRepository speciesRepository)
  {
    _context = context;
    _regionQuerier = regionQuerier;
    _speciesQuerier = speciesQuerier;
    _speciesRepository = speciesRepository;
  }

  public async Task<SpeciesAggregate> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    SpeciesId? speciesId;
    if (Guid.TryParse(idOrKey, out Guid id))
    {
      speciesId = new(worldId, id);
      SpeciesAggregate? species = await _speciesRepository.LoadAsync(speciesId.Value, cancellationToken);
      if (species is not null)
      {
        return species;
      }
    }

    speciesId = await _speciesQuerier.FindIdAsync(idOrKey, cancellationToken);
    if (!speciesId.HasValue)
    {
      throw new SpeciesNotFoundException(worldId, idOrKey, propertyName);
    }

    return await _speciesRepository.LoadAsync(speciesId.Value, cancellationToken)
      ?? throw new InvalidOperationException($"The species 'Id={speciesId}' was not loaded.");
  }

  public async Task<IReadOnlyDictionary<RegionId, Number?>> FindRegionalNumbersAsync(
    IEnumerable<RegionalNumberPayload> payloads,
    string propertyName,
    CancellationToken cancellationToken)
  {
    int capacity = payloads.Count();
    Dictionary<RegionId, Number?> regionalNumbers = new(capacity);

    if (capacity > 0)
    {
      IReadOnlyCollection<RegionKey> allKeys = await _regionQuerier.ListKeysAsync(cancellationToken);
      Dictionary<Guid, RegionId> ids = new(capacity: allKeys.Count);
      Dictionary<string, RegionId> keys = new(capacity: allKeys.Count);
      foreach (RegionKey key in allKeys)
      {
        ids[key.Id] = key.RegionId;
        keys[key.Key] = key.RegionId;
      }

      List<string> missing = new(capacity);
      foreach (RegionalNumberPayload payload in payloads)
      {
        string region = Slug.Normalize(payload.Region);
        Number? number = payload.Number == 0 ? null : new(payload.Number);
        if ((Guid.TryParse(region, out Guid id) && ids.TryGetValue(id, out RegionId regionId)) || keys.TryGetValue(region, out regionId))
        {
          regionalNumbers[regionId] = number;
        }
        else
        {
          missing.Add(payload.Region);
        }
      }

      if (missing.Count > 0)
      {
        throw new RegionsNotFoundException(_context.WorldId, missing, propertyName);
      }
    }

    return regionalNumbers.AsReadOnly();
  }
}
