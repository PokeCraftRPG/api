using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

internal record SearchWorldsQuery(SearchWorldsPayload Payload) : IQuery<SearchResults<WorldModel>>;

internal class SearchWorldsQueryHandler : IQueryHandler<SearchWorldsQuery, SearchResults<WorldModel>>
{
  private readonly IWorldRepository _worldRepository;

  public SearchWorldsQueryHandler(IWorldRepository worldRepository)
  {
    _worldRepository = worldRepository;
  }

  public async Task<SearchResults<WorldModel>> HandleAsync(SearchWorldsQuery query, CancellationToken cancellationToken)
  {
    return await _worldRepository.SearchAsync(query.Payload, cancellationToken);
  }
}
