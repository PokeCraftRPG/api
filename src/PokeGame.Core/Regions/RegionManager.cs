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
      // TODO(fpion): implement
    }

    return regionalNumbers.AsReadOnly();
  }
}
