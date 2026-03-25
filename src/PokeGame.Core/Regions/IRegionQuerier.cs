using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions;

public interface IRegionQuerier
{
  Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<RegionKey>> ListKeysAsync(CancellationToken cancellationToken = default);

  Task<RegionModel> ReadAsync(Region region, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(RegionId id, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
