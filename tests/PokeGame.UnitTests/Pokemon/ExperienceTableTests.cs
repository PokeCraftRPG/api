using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;

namespace PokeGame.Pokemon;

public class ExperienceTableTests : UnitTests
{
  [Theory(DisplayName = "It should return the expected tier for a level.")]
  [InlineData(1, 0)]
  [InlineData(4, 0)]
  [InlineData(5, 1)]
  [InlineData(19, 1)]
  [InlineData(20, 2)]
  [InlineData(49, 2)]
  [InlineData(50, 3)]
  [InlineData(100, 3)]
  public void Given_Level_When_GetTier_Then_ExpectedTier(int level, int expectedTier)
  {
    Assert.Equal(expectedTier, ExperienceTable.GetTier(level));
  }

  [Theory(DisplayName = "It should return the expected level for MediumFast experience.")]
  [InlineData(0, 1)]
  [InlineData(8, 2)]
  [InlineData(125, 5)]
  [InlineData(8000, 20)]
  [InlineData(125000, 50)]
  [InlineData(1000000, 100)]
  public void Given_MediumFastExperience_When_GetLevel_Then_ExpectedLevel(int experience, int expectedLevel)
  {
    Assert.Equal(expectedLevel, ExperienceTable.GetLevel(GrowthRate.MediumFast, experience));
  }

  [Fact(DisplayName = "It should return level 1 for zero experience on every growth rate.")]
  public void Given_ZeroExperience_When_GetLevel_Then_LevelOne()
  {
    foreach (GrowthRate growthRate in Enum.GetValues<GrowthRate>())
    {
      Assert.Equal(1, ExperienceTable.GetLevel(growthRate, experience: 0));
    }
  }

  [Fact(DisplayName = "It should return a threshold that matches GetLevel for that level.")]
  public void Given_Level_When_GetThreshold_Then_MatchesGetLevelBoundary()
  {
    int threshold = ExperienceTable.GetThreshold(GrowthRate.MediumFast, level: 4);

    Assert.Equal(5, ExperienceTable.GetLevel(GrowthRate.MediumFast, threshold));
    Assert.Equal(4, ExperienceTable.GetLevel(GrowthRate.MediumFast, threshold - 1));
  }

  [Fact(DisplayName = "It should throw ArgumentException when the growth rate is invalid.")]
  public void Given_InvalidGrowthRate_When_GetLevel_Then_ArgumentException()
  {
    ArgumentException exception = Assert.Throws<ArgumentException>(
      () => ExperienceTable.GetLevel((GrowthRate)99, experience: 0));
    Assert.Equal("growthRate", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when experience is negative.")]
  public void Given_NegativeExperience_When_GetLevel_Then_ArgumentOutOfRangeException()
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => ExperienceTable.GetLevel(GrowthRate.MediumFast, experience: -1));
  }

  [Theory(DisplayName = "It should throw ArgumentOutOfRangeException when the level is out of range.")]
  [InlineData(0)]
  [InlineData(101)]
  public void Given_InvalidLevel_When_GetThreshold_Then_ArgumentOutOfRangeException(int level)
  {
    Assert.Throws<ArgumentOutOfRangeException>(() => ExperienceTable.GetThreshold(GrowthRate.MediumFast, level));
  }
}
