using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;

namespace PokeGame.Pokemon;

public class SpecimenTierTests : UnitTests
{
  [Theory(DisplayName = "It should expose the tier matching the Pokémon level.")]
  [InlineData(1, 0)]
  [InlineData(4, 0)]
  [InlineData(5, 1)]
  [InlineData(19, 1)]
  [InlineData(20, 2)]
  [InlineData(49, 2)]
  [InlineData(50, 3)]
  [InlineData(100, 3)]
  public void Given_ExperienceForLevel_When_Create_Then_TierMatchesLevel(int level, int expectedTier)
  {
    Specimen pokemon = Catalog.CreatePokemon(experience: ExperienceForLevel(level));

    Assert.Equal(level, pokemon.Level);
    Assert.Equal(expectedTier, pokemon.Tier);
    Assert.Equal(ExperienceTable.GetTier(pokemon.Level), pokemon.Tier);
  }

  [Fact(DisplayName = "It should expose tier 0 for a newly created Pokémon.")]
  public void Given_DefaultExperience_When_Create_Then_TierZero()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    Assert.Equal(0, pokemon.Experience);
    Assert.Equal(1, pokemon.Level);
    Assert.Equal(0, pokemon.Tier);
  }

  private static int ExperienceForLevel(int level)
    => level <= 1 ? 0 : ExperienceTable.GetThreshold(GrowthRate.MediumSlow, level - 1);
}
