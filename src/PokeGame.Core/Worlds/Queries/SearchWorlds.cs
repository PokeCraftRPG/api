using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

internal record SearchWorldsQuery(SearchWorldsPayload Payload) : IQuery<SearchResults<WorldModel>>;

internal class SearchWorldsQueryHandler : IQueryHandler<SearchWorldsQuery, SearchResults<WorldModel>>
{
  private readonly IWorldQuerier _worldQuerier;

  public SearchWorldsQueryHandler(IWorldQuerier worldQuerier)
  {
    _worldQuerier = worldQuerier;
  }

  public async Task<SearchResults<WorldModel>> HandleAsync(SearchWorldsQuery query, CancellationToken cancellationToken)
  {
    return await _worldQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
