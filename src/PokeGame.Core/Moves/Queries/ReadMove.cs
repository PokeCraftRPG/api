using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record ReadMoveQuery(Guid Id) : IQuery<MoveModel?>;

internal class ReadMoveQueryHandler : IQueryHandler<ReadMoveQuery, MoveModel?>
{
  private readonly IMoveQuerier _moveQuerier;

  public ReadMoveQueryHandler(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task<MoveModel?> HandleAsync(ReadMoveQuery query, CancellationToken cancellationToken)
  {
    return await _moveQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
