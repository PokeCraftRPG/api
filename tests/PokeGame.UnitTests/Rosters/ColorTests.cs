using PokeGame.Core.Rosters;

namespace PokeGame.Rosters;

public class ColorTests : UnitTests
{
  [Fact(DisplayName = "It should copy color components from another color.")]
  public void Given_Color_When_From_Then_Copied()
  {
    ColorDto source = new(12, 34, 56);

    Color color = Color.From(source);

    Assert.Equal((byte)12, color.Red);
    Assert.Equal((byte)34, color.Green);
    Assert.Equal((byte)56, color.Blue);
  }

  [Fact(DisplayName = "It should format the color as a hexadecimal string.")]
  public void Given_Color_When_ToString_Then_Hex()
  {
    Color color = new(255, 10, 0);

    Assert.Equal("#FF0A00", color.ToString());
  }

  private sealed record ColorDto(byte Red, byte Green, byte Blue) : IColor;
}
