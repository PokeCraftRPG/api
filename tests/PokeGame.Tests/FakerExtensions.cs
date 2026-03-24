using Bogus;
using PokeGame.Core.Species;

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
}
