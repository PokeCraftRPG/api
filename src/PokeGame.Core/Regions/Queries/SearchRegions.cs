using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

internal record SearchRegionsQuery(SearchRegionsPayload Payload) : IQuery<SearchResults<RegionDto>>;

internal class SearchRegionsQueryHandler : IQueryHandler<SearchRegionsQuery, SearchResults<RegionDto>>
{
  private readonly IRegionQuerier _regionQuerier;

  public SearchRegionsQueryHandler(IRegionQuerier regionQuerier)
  {
    _regionQuerier = regionQuerier;
  }

  public async Task<SearchResults<RegionDto>> HandleAsync(SearchRegionsQuery query, CancellationToken cancellationToken)
  {
    SearchRegionsPayload payload = query.Payload;
    payload.Validate();

    return await _regionQuerier.SearchAsync(payload, cancellationToken);
  }
}
