using Krakenar.Contracts.Search;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves;

public interface IMoveQuerier
{
  Task<MoveId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<MoveDto> ReadAsync(Move move, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(MoveId id, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<MoveDto>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken = default);
}
