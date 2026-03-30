using Krakenar.Contracts;
using Krakenar.Contracts.Actors;

namespace PokeGame.Core.Trainers.Models;

public class TrainerModel : Aggregate
{
  public string License { get; set; } = string.Empty;

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public TrainerGender Gender { get; set; }
  public int Money { get; set; }
  public int PartySize { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }

  public Actor? Owner { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
