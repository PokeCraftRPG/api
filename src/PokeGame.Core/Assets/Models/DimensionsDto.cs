namespace PokeGame.Core.Assets.Models;

public record DimensionsDto : IDimensions
{
  public int Width { get; set; }
  public int Height { get; set; }

  public DimensionsDto()
  {
  }

  [JsonConstructor]
  public DimensionsDto(int width, int height)
  {
    Width = width;
    Height = height;
  }

  public DimensionsDto(IDimensions dimensions) : this(dimensions.Width, dimensions.Height)
  {
  }
}
