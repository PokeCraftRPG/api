namespace PokeGame.Core.Moves.Models;

public class MoveSummary
{
  public Guid Id { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }

  public MoveSummary()
  {
  }

  public MoveSummary(Guid id, string key, string? name)
  {
    Id = id;

    Key = key;

    Name = name;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
