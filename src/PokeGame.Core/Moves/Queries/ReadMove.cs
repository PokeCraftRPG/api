using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

internal record ReadMoveQuery(Guid? Id, string? Key) : IQuery<MoveModel?>;

internal class ReadMoveQueryHandler : IQueryHandler<ReadMoveQuery, MoveModel?>
{
  private readonly IMoveRepository _moveRepository;

  public ReadMoveQueryHandler(IMoveRepository moveRepository)
  {
    _moveRepository = moveRepository;
  }

  public async Task<MoveModel?> HandleAsync(ReadMoveQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, MoveModel> moves = new(capacity: 2);

    if (query.Id.HasValue)
    {
      MoveModel? move = await _moveRepository.ReadAsync(query.Id.Value, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      MoveModel? move = await _moveRepository.ReadAsync(query.Key, cancellationToken);
      if (move is not null)
      {
        moves[move.Id] = move;
      }
    }

    if (moves.Count > 1)
    {
      throw TooManyResultsException<MoveModel>.ExpectedSingle(moves.Count);
    }

    return moves.SingleOrDefault().Value;
  }
}
