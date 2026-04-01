using Krakenar.Contracts.Search;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves;

public interface IMoveQuerier
{
  Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken = default);

  Task<MoveId?> FindIdAsync(string key, CancellationToken cancellationToken = default);

  Task<IReadOnlyCollection<MoveKey>> ListKeysAsync(CancellationToken cancellationToken = default);

  Task<MoveModel> ReadAsync(Move move, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(MoveId id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken = default);
}
