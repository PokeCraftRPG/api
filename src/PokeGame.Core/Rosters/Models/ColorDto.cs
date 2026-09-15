namespace PokeGame.Core.Rosters.Models;

public record ColorDto : IColor
{
  public byte Red { get; set; }
  public byte Green { get; set; }
  public byte Blue { get; set; }

  public ColorDto()
  {
  }

  public ColorDto(byte red, byte green, byte blue)
  {
    Red = red;
    Green = green;
    Blue = blue;
  }

  public ColorDto(IColor color) : this(color.Red, color.Green, color.Blue)
  {
  }
}
