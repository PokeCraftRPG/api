using Logitar.EventSourcing;
using PokeGame.Core.Regions.Events;

namespace PokeGame.Core.Regions;

public interface IRegionManager
{
  Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken = default);
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
}
