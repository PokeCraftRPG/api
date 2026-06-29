namespace PokeGame.Core.Regions.Events;

public class RegionCreated : CreateEvent
{
  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public RegionCreated() : base()
  {
  }

  public RegionCreated(Region region) : base(region)
  {
    Key = region.Key;
    Name = region.Name;
    Description = region.Description;
  }
}
