namespace PokeGame.Core.Abilities.Models;

public class AbilitySummary
{
  public Guid Id { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }

  public AbilitySummary()
  {
  }

  public AbilitySummary(Guid id, string key, string? name)
  {
    Id = id;

    Key = key;

    Name = name;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
