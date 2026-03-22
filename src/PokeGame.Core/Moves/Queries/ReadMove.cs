using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record ReadMoveQuery(Guid? Id, string? Key) : IQuery<MoveModel?>;

internal class ReadMoveQueryHandler : IQueryHandler<ReadMoveQuery, MoveModel?>
{
  private readonly IMoveQuerier _moveQuerier;

  public ReadMoveQueryHandler(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task<MoveModel?> HandleAsync(ReadMoveQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, MoveModel> moves = new(capacity: 2);

    if (query.Id.HasValue)
    {
      MoveModel? move = await _moveQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      MoveModel? move = await _moveQuerier.ReadAsync(query.Key, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (moves.Count > 1)
    {
      throw TooManyResultsException<MoveModel>.ExpectedSingle(moves.Count);
    }

    return moves.Values.SingleOrDefault();
  }
}
