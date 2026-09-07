using Krakenar.Contracts;
using Krakenar.Contracts.Actors;
using PokeGame.Core.Assets.Models;

namespace PokeGame.Core.Trainers.Models;

public class TrainerDto : Aggregate
{
  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public string? License { get; set; }
  public Gender? Gender { get; set; }
  public int Money { get; set; }
  public AssetDto? Sprite { get; set; }

  public Actor? Member { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
