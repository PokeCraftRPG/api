using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon;

public interface IPokemonRandomizer
{
  PokemonGender? Gender(GenderRatio? genderRatio);
}

public class PokemonRandomizer : IPokemonRandomizer
{
  private static IPokemonRandomizer? _instance = null;
  public static IPokemonRandomizer Instance
  {
    get
    {
      _instance ??= new PokemonRandomizer();
      return _instance;
    }
  }

  private readonly Random _random = new();

  private PokemonRandomizer()
  {
  }

  public PokemonGender? Gender(GenderRatio? genderRatio)
  {
    if (genderRatio is null)
    {
      return null;
    }

    int value = _random.Next(0, GenderRatio.MaximumValue);
    return value < genderRatio.Value ? PokemonGender.Male : PokemonGender.Female;
  }
}
