namespace PokeGame.Core.Forms.Models;

public record FormSizeDto : IFormSize
{
  public int Height { get; set; }
  public int Weight { get; set; }

  public FormSizeDto()
  {
  }

  public FormSizeDto(int height, int weight)
  {
    Height = height;
    Weight = weight;
  }

  public FormSizeDto(IFormSize size) : this(size.Height, size.Weight)
  {
  }
}
