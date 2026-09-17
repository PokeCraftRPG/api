namespace PokeGame.Core.Species.Models;

public class SpeciesSummary
{
  public Guid Id { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }

  public SpeciesSummary()
  {
  }

  public SpeciesSummary(Guid id, string key, string? name)
  {
    Id = id;

    Key = key;

    Name = name;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
