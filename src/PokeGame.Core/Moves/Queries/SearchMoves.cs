using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record SearchMovesQuery(SearchMovesPayload Payload) : IQuery<SearchResults<MoveModel>>;

internal class SearchMovesQueryHandler : IQueryHandler<SearchMovesQuery, SearchResults<MoveModel>>
{
  private readonly IMoveQuerier _moveQuerier;

  public SearchMovesQueryHandler(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task<SearchResults<MoveModel>> HandleAsync(SearchMovesQuery query, CancellationToken cancellationToken)
  {
    return await _moveQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
