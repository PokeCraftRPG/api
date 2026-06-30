using Krakenar.Contracts.Search;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves;

public interface IMoveRepository
{
  void Add(params Move[] moves);
  void Remove(Move move);
  void Update(Move move, MoveUpdated record);

  Task EnsureUnicityAsync(Move move, CancellationToken cancellationToken = default);

  Task<Move?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<MoveModel> ReadAsync(Move move, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<MoveModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<MoveModel>> SearchAsync(SearchMovesPayload payload, CancellationToken cancellationToken = default);
}
