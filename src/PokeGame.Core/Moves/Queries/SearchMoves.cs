using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record SearchMovesQuery(SearchMovesPayload Payload) : IQuery<SearchResults<MoveModel>>;

internal class SearchMovesQueryHandler : IQueryHandler<SearchMovesQuery, SearchResults<MoveModel>>
{
  private readonly IMoveRepository _moveRepository;

  public SearchMovesQueryHandler(IMoveRepository moveRepository)
  {
    _moveRepository = moveRepository;
  }

  public async Task<SearchResults<MoveModel>> HandleAsync(SearchMovesQuery query, CancellationToken cancellationToken)
  {
    return await _moveRepository.SearchAsync(query.Payload, cancellationToken);
  }
}
