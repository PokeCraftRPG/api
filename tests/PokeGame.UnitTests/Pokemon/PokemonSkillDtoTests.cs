using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Pokemon;

public class PokemonSkillDtoTests : UnitTests
{
  [Theory(DisplayName = "It should compute the skill total from effective rank, attribute and modifiers.")]
  [InlineData(0, 0, 0, 0, 0)]
  [InlineData(1, 2, 3, 0, 6)]
  [InlineData(1, 4, 2, 1, 7)]
  [InlineData(2, 5, 0, 0, 7)]
  [InlineData(4, 10, 5, -2, 17)]
  public void Given_Components_When_Total_Then_EffectiveRankPlusAttributePlusModifiers(
    int training,
    int rank,
    int attribute,
    int modifiers,
    int expectedTotal)
  {
    PokemonSkillDto skill = new()
    {
      Training = training,
      Rank = rank,
      Attribute = attribute,
      Modifiers = modifiers
    };

    Assert.Equal(new PokemonSkillTraining(training, rank).GetEffectiveRank() + attribute + modifiers, skill.Total);
    Assert.Equal(expectedTotal, skill.Total);
  }
}
