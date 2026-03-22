using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves;

public interface IMoveQuerier
{
  Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken = default);

  Task<MoveModel> ReadAsync(Move move, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(MoveId id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
