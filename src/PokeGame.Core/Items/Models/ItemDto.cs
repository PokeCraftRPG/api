using Krakenar.Contracts;
using PokeGame.Core.Assets.Models;

namespace PokeGame.Core.Items.Models;

public class ItemDto : Aggregate
{
  public ItemCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int? Price { get; set; }
  public int? Weight { get; set; }

  public AssetDto? Sprite { get; set; }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
