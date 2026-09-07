using Krakenar.Contracts;

namespace PokeGame.Core.Trainers.Models;

public class TrainerDto : Aggregate
{
  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  // TODO(fpion): License
  // TODO(fpion): Gender
  // TODO(fpion): Money
  // TODO(fpion): Sprite
  // TODO(fpion): User/Member

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
