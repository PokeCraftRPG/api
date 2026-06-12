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
    Slug? key = null;

    foreach (IEvent change in world.Changes)
    {
      if (change is WorldCreated || change is WorldKeyChanged)
      {
        key = world.Key;
      }
    }

    if (key is not null)
    {
      WorldId? otherId = await _worldQuerier.FindIdAsync(key, cancellationToken);
      if (otherId.HasValue && !otherId.Value.Equals(world.Id))
      {
        throw new NotImplementedException(); // TODO(fpion): 409 Conflict
      }
    }
  }
}
