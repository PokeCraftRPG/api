namespace PokeGame.Core.Regions;

public interface IRegionRepository
{
  Task<Region?> LoadAsync(RegionId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<Region>> LoadAsync(IEnumerable<RegionId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(Region region, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<Region> regions, CancellationToken cancellationToken = default);
}
