using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

internal record SearchRegionsQuery(SearchRegionsPayload Payload) : IQuery<SearchResults<RegionModel>>;

internal class SearchRegionsQueryHandler : IQueryHandler<SearchRegionsQuery, SearchResults<RegionModel>>
{
  private readonly IRegionRepository _regionRepository;

  public SearchRegionsQueryHandler(IRegionRepository regionRepository)
  {
    _regionRepository = regionRepository;
  }

  public async Task<SearchResults<RegionModel>> HandleAsync(SearchRegionsQuery query, CancellationToken cancellationToken)
  {
    return await _regionRepository.SearchAsync(query.Payload, cancellationToken);
  }
}
