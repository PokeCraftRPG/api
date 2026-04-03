using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

internal record SearchVarietiesQuery(SearchVarietiesPayload Payload) : IQuery<SearchResults<VarietyModel>>;

internal class SearchVarietiesQueryHandler : IQueryHandler<SearchVarietiesQuery, SearchResults<VarietyModel>>
{
  private readonly IVarietyQuerier _varietyQuerier;

  public SearchVarietiesQueryHandler(IVarietyQuerier varietyQuerier)
  {
    _varietyQuerier = varietyQuerier;
  }

  public async Task<SearchResults<VarietyModel>> HandleAsync(SearchVarietiesQuery query, CancellationToken cancellationToken)
  {
    return await _varietyQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
