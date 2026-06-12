using Krakenar.Contracts.Search;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds;

public interface IWorldQuerier
{
  Task<int> CountAsync(CancellationToken cancellationToken = default);

  Task<WorldModel> FindAsync(World world, CancellationToken cancellationToken = default);
  Task<WorldModel> FindAsync(WorldId id, CancellationToken cancellationToken = default);

  Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<WorldModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<WorldModel>> SearchAsync(SearchWorldsPayload payload, CancellationToken cancellationToken = default);

  Task<WorldId?> TryGetIdAsync(Slug key, CancellationToken cancellationToken = default);
}
