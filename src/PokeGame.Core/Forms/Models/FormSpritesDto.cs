using PokeGame.Core.Assets.Models;

namespace PokeGame.Core.Forms.Models;

public class FormSpritesDto
{
  public AssetDto Default { get; set; } = new();
  public AssetDto? Shiny { get; set; }
  public AssetDto? Female { get; set; }
  public AssetDto? FemaleShiny { get; set; }
}
