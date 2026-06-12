namespace PokeGame.Core.Worlds;

public interface IWorldRepository
{
  Task<World?> LoadAsync(WorldId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<World>> LoadAsync(IEnumerable<WorldId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(World world, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<World> worlds, CancellationToken cancellationToken = default);
}
