namespace PokeGame.Core.Worlds.Events;

public class WorldUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public WorldUpdated() : base()
  {
  }

  public WorldUpdated(World world) : base(world)
  {
  }
}
