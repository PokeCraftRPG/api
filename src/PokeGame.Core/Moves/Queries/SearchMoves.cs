using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record SearchMovesQuery(SearchMovesPayload Payload) : IQuery<SearchResults<MoveDto>>;

internal class SearchMovesQueryHandler : IQueryHandler<SearchMovesQuery, SearchResults<MoveDto>>
{
  private readonly IMoveQuerier _moveQuerier;

  public SearchMovesQueryHandler(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task<SearchResults<MoveDto>> HandleAsync(SearchMovesQuery query, CancellationToken cancellationToken)
  {
    SearchMovesPayload payload = query.Payload;
    payload.Validate();

    return await _moveQuerier.SearchAsync(payload, cancellationToken);
  }
}
