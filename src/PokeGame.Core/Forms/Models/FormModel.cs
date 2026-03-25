using Krakenar.Contracts;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Forms.Models;

public class FormModel : Aggregate
{
  public VarietyModel Variety { get; set; } = new();
  public bool IsDefault { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public bool IsBattleOnly { get; set; }
  public bool IsMega { get; set; }

  public int Height { get; set; }
  public int Weight { get; set; }

  public FormTypesModel Types { get; set; } = new();
  // TODO(fpion): Abilities
  // TODO(fpion): BaseStatistics
  // TODO(fpion): Yield
  // TODO(fpion): Sprites

  public string? Url { get; set; }
  public string? Note { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
