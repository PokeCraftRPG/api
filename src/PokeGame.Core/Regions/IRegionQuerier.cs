using Krakenar.Contracts.Search;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions;

public interface IRegionQuerier
{
  Task<RegionModel> FindAsync(Region region, CancellationToken cancellationToken = default);
  Task<RegionModel> FindAsync(RegionId id, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<RegionKey>> ListKeysAsync(CancellationToken cancellationToken = default);

  Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<RegionModel>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken = default);

  Task<RegionId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken = default);
}
