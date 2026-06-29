namespace PokeGame.Core.Regions.Events;

public class RegionDeleted : DeleteEvent
{
  public RegionDeleted() : base()
  {
  }

  public RegionDeleted(Region region, Guid userId) : base(region, userId)
  {
  }
}
