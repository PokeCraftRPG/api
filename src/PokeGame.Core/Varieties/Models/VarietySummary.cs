namespace PokeGame.Core.Varieties.Models;

public class VarietySummary
{
  public Guid Id { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }

  public VarietySummary()
  {
  }

  public VarietySummary(Guid id, string key, string? name)
  {
    Id = id;

    Key = key;

    Name = name;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
