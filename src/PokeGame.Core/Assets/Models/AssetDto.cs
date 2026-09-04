using Krakenar.Contracts;

namespace PokeGame.Core.Assets.Models;

public class AssetDto : Aggregate
{
  public AssetKind Kind { get; set; }

  public FileDto File { get; set; } = new();
  public DimensionsDto? Dimensions { get; set; }
  public TimeSpan? Duration { get; set; }

  public override string ToString() => $"{File.Name}.{File.Extension} | {base.ToString()}";
}
