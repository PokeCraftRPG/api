using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions;

public interface IRegionQuerier
{
  Task<RegionId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<RegionDto> ReadAsync(Region region, CancellationToken cancellationToken = default);
  Task<RegionDto?> ReadAsync(RegionId id, CancellationToken cancellationToken = default);
  Task<RegionDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<RegionDto?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
