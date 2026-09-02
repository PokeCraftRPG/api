using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record ReadMoveQuery(Guid? Id, string? Key) : IQuery<MoveDto?>;

internal class ReadMoveQueryHandler : IQueryHandler<ReadMoveQuery, MoveDto?>
{
  private readonly IMoveQuerier _moveQuerier;

  public ReadMoveQueryHandler(IMoveQuerier moveQuerier)
  {
    _moveQuerier = moveQuerier;
  }

  public async Task<MoveDto?> HandleAsync(ReadMoveQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, MoveDto> moves = new(capacity: 2);

    if (query.Id.HasValue)
    {
      MoveDto? move = await _moveQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      MoveDto? move = await _moveQuerier.ReadAsync(query.Key, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (moves.Count > 1)
    {
      throw TooManyResultsException<MoveDto>.ExpectedSingle(moves.Count);
    }

    return moves.Values.SingleOrDefault();
  }
}
