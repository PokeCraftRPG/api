using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds;

public interface IWorldQuerier
{
  Task<WorldModel> ReadAsync(World world, CancellationToken cancellationToken = default);
  Task<WorldModel?> ReadAsync(WorldId id, CancellationToken cancellationToken = default);
  Task<WorldModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<WorldModel?> ReadAsync(string slug, CancellationToken cancellationToken = default);
}
