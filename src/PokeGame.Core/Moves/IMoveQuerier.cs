using Krakenar.Contracts.Search;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves;

public interface IMoveQuerier
{
  Task<MoveModel> FindAsync(Move move, CancellationToken cancellationToken = default);
  Task<MoveModel> FindAsync(MoveId id, CancellationToken cancellationToken = default);

  Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken = default);

  Task<MoveId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken = default);
}
