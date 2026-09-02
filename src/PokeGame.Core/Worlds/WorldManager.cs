using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public interface IWorldManager
{
  Task EnsureUnicityAsync(World world, CancellationToken cancellationToken = default);
}

internal class WorldManager : IWorldManager
{
  private readonly IWorldQuerier _worldQuerier;

  public WorldManager(IWorldQuerier worldQuerier)
  {
    _worldQuerier = worldQuerier;
  }

  public async Task EnsureUnicityAsync(World world, CancellationToken cancellationToken)
  {
    Key? key = null;
    foreach (IEvent change in world.Changes)
    {
      if (change is WorldCreated created)
      {
        key = created.Key;
      }
      else if (change is WorldKeyChanged changed)
      {
        key = changed.Key;
      }
    }

    if (key is not null)
    {
      WorldId? worldId = await _worldQuerier.GetIdAsync(key, cancellationToken);
      if (worldId.HasValue && !worldId.Value.Equals(world.Id))
      {
        throw new KeyAlreadyUsedException(world, worldId.Value.EntityId, world.Key, nameof(world.Key));
      }
    }
  }
}
