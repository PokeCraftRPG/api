namespace PokeGame.Core.Regions.Events;

public class RegionUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public RegionUpdated() : base()
  {
  }

  public RegionUpdated(Region region) : base(region)
  {
  }
}
