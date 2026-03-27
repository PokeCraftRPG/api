using Krakenar.Contracts;

namespace PokeGame.Core.Trainers.Models;

public class TrainerModel : Aggregate
{
  public Guid? UserId { get; set; }

  public string License { get; set; } = string.Empty;

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public TrainerGender Gender { get; set; }
  public int Money { get; set; }
  public int PartySize { get; set; } // TODO(fpion): is this a computed?

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
