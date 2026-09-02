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
    Key? key = null;
    foreach (IEvent change in region.Changes)
    {
      if (change is RegionCreated created)
      {
        key = created.Key;
      }
      else if (change is RegionKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      RegionId? regionId = await _regionQuerier.GetIdAsync(key, cancellationToken);
      if (regionId.HasValue && !regionId.Value.Equals(region.Id))
      {
        throw new KeyAlreadyUsedException(region, regionId.Value.EntityId, region.Key, nameof(region.Key));
      }
    }
  }
}
