using Krakenar.Contracts.Search;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds;

public interface IWorldQuerier
{
  Task<int> CountAsync(CancellationToken cancellationToken = default);

  Task<WorldId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<WorldDto> ReadAsync(World world, CancellationToken cancellationToken = default);
  Task<WorldDto?> ReadAsync(WorldId id, CancellationToken cancellationToken = default);
  Task<WorldDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<WorldDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<WorldDto>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken = default);
}
