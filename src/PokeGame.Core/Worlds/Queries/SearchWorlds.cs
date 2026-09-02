using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

internal record SearchWorldsQuery(SearchWorldsPayload Payload) : IQuery<SearchResults<WorldDto>>;

internal class SearchWorldsQueryHandler : IQueryHandler<SearchWorldsQuery, SearchResults<WorldDto>>
{
  private readonly IWorldQuerier _worldQuerier;

  public SearchWorldsQueryHandler(IWorldQuerier worldQuerier)
  {
    _worldQuerier = worldQuerier;
  }

  public async Task<SearchResults<WorldDto>> HandleAsync(SearchWorldsQuery query, CancellationToken cancellationToken)
  {
    SearchWorldsPayload payload = query.Payload;
    payload.Validate();

    return await _worldQuerier.SearchAsync(payload, cancellationToken);
  }
}
