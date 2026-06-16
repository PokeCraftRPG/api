using Logitar.EventSourcing;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Regions;

public interface IRegionManager
{
  Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken = default);
  Task<IReadOnlyDictionary<RegionId, Number?>> FindRegionalNumbersAsync(IEnumerable<RegionalNumberPayload> payloads, string propertyName, CancellationToken cancellationToken = default);
}

internal class RegionManager : IRegionManager
{
  private readonly IRegionQuerier _regionQuerier;

  public RegionManager(IRegionQuerier regionQuerier)
  {
    _regionQuerier = regionQuerier;
  }

  public async Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken)
  {
    Slug? key = null;

    foreach (IEvent change in region.Changes)
    {
      if (change is RegionCreated || change is RegionKeyChanged)
      {
        key = region.Key;
      }
    }

    if (key is not null)
    {
      RegionId? otherId = await _regionQuerier.TryGetIdAsync(key, cancellationToken);
      if (otherId.HasValue && !otherId.Value.Equals(region.Id))
      {
        throw new KeyAlreadyUsedException(region, otherId.Value.EntityId, key, nameof(region.Key));
      }
    }
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
      IReadOnlyCollection<RegionKey> keys = await _regionQuerier.ListKeysAsync(cancellationToken);
      Dictionary<Guid, RegionId> regionByIds = new(keys.Count);
      Dictionary<string, RegionId> regionByKeys = new(capacity);
      foreach (RegionKey key in keys)
      {
        regionByIds[key.EntityId] = key.Id;
        regionByKeys[key.Key] = key.Id;
      }

      HashSet<string> missingRegions = new(capacity);
      foreach (RegionalNumberPayload payload in payloads)
      {
        if ((Guid.TryParse(payload.Region, out Guid id) && regionByIds.TryGetValue(id, out RegionId regionId))
          || regionByKeys.TryGetValue(Slug.Normalize(payload.Region), out regionId))
        {
          regionalNumbers[regionId] = Number.TryCreate(payload.Number);
        }
        else
        {
          missingRegions.Add(payload.Region);
        }
      }

      if (missingRegions.Count > 0)
      {
        throw new RegionsNotFoundException(missingRegions, propertyName);
      }
    }

    return regionalNumbers.AsReadOnly();
  }
}
