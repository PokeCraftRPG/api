using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

internal record SearchVarietiesQuery(SearchVarietiesPayload Payload) : IQuery<SearchResults<VarietyDto>>;

internal class SearchVarietiesQueryHandler : IQueryHandler<SearchVarietiesQuery, SearchResults<VarietyDto>>
{
  private readonly IVarietyQuerier _varietyQuerier;

  public SearchVarietiesQueryHandler(IVarietyQuerier varietyQuerier)
  {
    _varietyQuerier = varietyQuerier;
  }

  public async Task<SearchResults<VarietyDto>> HandleAsync(SearchVarietiesQuery query, CancellationToken cancellationToken)
  {
    SearchVarietiesPayload payload = query.Payload;
    payload.Validate();

    return await _varietyQuerier.SearchAsync(payload, cancellationToken);
  }
}
