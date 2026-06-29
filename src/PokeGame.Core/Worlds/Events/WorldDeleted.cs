namespace PokeGame.Core.Worlds.Events;

public class WorldDeleted : DeleteEvent
{
  public WorldDeleted() : base()
  {
  }

  public WorldDeleted(World world, Guid userId) : base(world, userId)
  {
  }
}
