namespace PokeGame.Core.Regions.Models;

public class RegionSummary
{
  public Guid Id { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }

  public RegionSummary()
  {
  }

  public RegionSummary(Guid id, string key, string? name)
  {
    Id = id;

    Key = key;

    Name = name;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
