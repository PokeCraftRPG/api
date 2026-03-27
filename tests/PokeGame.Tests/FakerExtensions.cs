using Bogus;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;

namespace PokeGame;

public static class FakerExtensions
{
  public static EggGroups EggGroups(this Faker faker)
  {
    EggGroup primary = faker.PickRandom<EggGroup>();
    EggGroup? secondary = faker.PickRandom<EggGroup>();
    if (primary == secondary || primary == EggGroup.NoEggsDiscovered || primary == EggGroup.Ditto || secondary == EggGroup.NoEggsDiscovered || secondary == EggGroup.Ditto)
    {
      secondary = null;
    }
    return new EggGroups(primary, secondary);
  }

  public static License TrainerLicense(this Faker faker)
  {
    string number = faker.Random.String(6, '0', '9');
    return new License(string.Join('_', 'Q', number, Hash(number)));
  }
  private static string Hash(string number)
  {
    if (number.Length == 1)
    {
      return number;
    }

    int sum = 0;
    foreach (char c in number)
    {
      if (!char.IsDigit(c))
      {
        throw new ArgumentOutOfRangeException(nameof(number));
      }
      sum += c - '0';
    }

    return Hash(sum.ToString());
  }
}
