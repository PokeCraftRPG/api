namespace PokeGame.Core.Pokemon;

public interface IPokemonSize
{
  byte Scale { get; }
  SizeCategory Category { get; }
}

public sealed record PokemonSize : IPokemonSize
{
  public byte Scale { get; }
  public SizeCategory Category { get; }

  public PokemonSize(byte scale)
  {
    Scale = scale;
    Category = CalculateCategory(scale);
  }

  public static SizeCategory CalculateCategory(byte scale)
  {
    if (scale < 16)
    {
      return SizeCategory.ExtraSmall;
    }
    if (scale < 48)
    {
      return SizeCategory.Small;
    }
    if (scale >= 240)
    {
      return SizeCategory.ExtraLarge;
    }
    if (scale >= 208)
    {
      return SizeCategory.Large;
    }
    return SizeCategory.Medium;
  }
}
