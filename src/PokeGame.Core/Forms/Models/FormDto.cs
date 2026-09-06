using Krakenar.Contracts;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Forms.Models;

public class FormDto : Aggregate
{
  public VarietyDto Variety { get; set; } = new();
  public FormCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public FormTypesDto Types { get; set; } = new();
  public FormAbilitiesDto Abilities { get; set; } = new();
  public BaseStatisticsDto BaseStatistics { get; set; } = new();
  public FormYieldDto Yield { get; set; } = new();

  public FormSizeDto? Size { get; set; }
  public FormSpritesDto? Sprites { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
