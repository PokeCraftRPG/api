namespace PokeGame.Core.Forms.Models;

public record FormSizeDto : IFormSize
{
  public int Height { get; set; }
  public int Weight { get; set; }
}
