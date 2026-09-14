using FluentValidation;
using PokeGame.Core.Pokemon;

namespace PokeGame.Pokemon;

public class PokemonSkillTrainingTests : UnitTests
{
  [Theory(DisplayName = "It should accept valid level and rank bounds.")]
  [InlineData(0, 0)]
  [InlineData(0, 10)]
  [InlineData(4, 0)]
  [InlineData(4, 10)]
  [InlineData(2, 5)]
  public void Given_ValidBounds_When_Create_Then_Created(int level, int rank)
  {
    PokemonSkillTraining training = new(level, rank);

    Assert.Equal(level, training.Level);
    Assert.Equal(rank, training.Rank);
  }

  [Theory(DisplayName = "It should reject an invalid level.")]
  [InlineData(-1, 0)]
  [InlineData(5, 0)]
  public void Given_InvalidLevel_When_Create_Then_ValidationException(int level, int rank)
  {
    Assert.Throws<ValidationException>(() => new PokemonSkillTraining(level, rank));
  }

  [Theory(DisplayName = "It should reject an invalid rank.")]
  [InlineData(0, -1)]
  [InlineData(0, 11)]
  public void Given_InvalidRank_When_Create_Then_ValidationException(int level, int rank)
  {
    Assert.Throws<ValidationException>(() => new PokemonSkillTraining(level, rank));
  }

  [Theory(DisplayName = "It should use the full rank while under the trained threshold.")]
  [InlineData(0, 0, 0)]
  [InlineData(1, 0, 1)]
  [InlineData(1, 2, 3)]
  [InlineData(2, 5, 7)]
  [InlineData(3, 9, 12)]
  [InlineData(4, 10, 14)]
  public void Given_RankAtOrBelowTrained_When_GetEffectiveRank_Then_LevelPlusRank(int level, int rank, int expected)
  {
    Assert.Equal(expected, new PokemonSkillTraining(level, rank).GetEffectiveRank());
  }

  [Theory(DisplayName = "It should halve ranks above the trained threshold.")]
  [InlineData(0, 1, 0)]
  [InlineData(0, 2, 1)]
  [InlineData(0, 3, 1)]
  [InlineData(1, 3, 3)]
  [InlineData(1, 4, 4)]
  [InlineData(1, 5, 4)]
  [InlineData(2, 6, 7)]
  [InlineData(2, 7, 8)]
  [InlineData(2, 8, 8)]
  [InlineData(3, 10, 12)]
  public void Given_RankAboveTrained_When_GetEffectiveRank_Then_HalvedOverflow(int level, int rank, int expected)
  {
    Assert.Equal(expected, new PokemonSkillTraining(level, rank).GetEffectiveRank());
  }
}
