using Krakenar.Contracts.Search;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions;

public interface IRegionRepository
{
  void Add(params Region[] regions);
  void Remove(Region region);
  void Update(Region region, RegionUpdated record);

  Task EnsureUnicityAsync(Region region, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<Region>> LoadAsync(CancellationToken cancellationToken = default);
  Task<Region?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<RegionModel> ReadAsync(Region region, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<RegionModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<RegionModel>> SearchAsync(SearchRegionsPayload payload, CancellationToken cancellationToken = default);
}
