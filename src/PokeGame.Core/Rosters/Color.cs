namespace PokeGame.Core.Rosters;

public interface IColor
{
  byte Red { get; }
  byte Green { get; }
  byte Blue { get; }
}

public record Color(byte Red, byte Green, byte Blue) : IColor
{
  public static Color From(IColor color) => new(color.Red, color.Green, color.Blue);

  public override string ToString() => string.Concat('#', Red.ToString("X2"), Green.ToString("X2"), Blue.ToString("X2"));
}
