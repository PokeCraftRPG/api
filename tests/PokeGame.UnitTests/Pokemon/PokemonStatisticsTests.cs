using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;

namespace PokeGame.Pokemon;

public class PokemonStatisticsTests : UnitTests
{
  [Theory(DisplayName = "It should calculate vitality and stamina with the HP formula.")]
  [InlineData(45, 0, 0, 1, 11)]
  [InlineData(45, 31, 0, 1, 12)]
  [InlineData(45, 0, 0, 50, 105)]
  [InlineData(45, 31, 252, 50, 152)]
  [InlineData(100, 31, 252, 100, 404)]
  public void Given_HpInputs_When_Create_Then_VitalityAndStaminaCalculated(
    byte @base,
    byte individual,
    byte effort,
    int level,
    int expected)
  {
    PokemonStatistics statistics = new(
      new BaseStatistics(@base, 10, 10, 10, 10, 10),
      new IndividualValues(individual, 0, 0, 0, 0, 0),
      new TestEffortValues(Vitality: effort, Stamina: effort),
      level,
      PokemonNatures.Hardy);

    Assert.Equal(expected, statistics.Vitality);
    Assert.Equal(expected, statistics.Stamina);
  }

  [Fact(DisplayName = "It should calculate vitality and stamina independently from different effort values.")]
  public void Given_DifferentEffortValues_When_Create_Then_VitalityAndStaminaDiffer()
  {
    PokemonStatistics statistics = new(
      new BaseStatistics(45, 10, 10, 10, 10, 10),
      new IndividualValues(0, 0, 0, 0, 0, 0),
      new TestEffortValues(Vitality: 0, Stamina: 252),
      level: 50,
      PokemonNatures.Hardy);

    Assert.Equal(105, statistics.Vitality);
    Assert.Equal(136, statistics.Stamina);
  }

  [Theory(DisplayName = "It should apply the nature multiplier to other statistics.")]
  [InlineData("Adamant", 75)]
  [InlineData("Modest", 62)]
  [InlineData("Hardy", 69)]
  public void Given_Nature_When_Create_Then_AttackCalculated(string natureName, int expected)
  {
    PokemonNature nature = PokemonNatures.Find(natureName);
    PokemonStatistics statistics = new(
      new BaseStatistics(45, 49, 10, 10, 10, 10),
      new IndividualValues(0, 31, 0, 0, 0, 0),
      new TestEffortValues(),
      level: 50,
      nature);

    Assert.Equal(expected, statistics.Attack);
  }

  [Fact(DisplayName = "It should calculate all battle statistics from inputs.")]
  public void Given_Inputs_When_Create_Then_AllStatisticsCalculated()
  {
    BaseStatistics baseStatistics = new(45, 49, 49, 65, 65, 45);
    IndividualValues individualValues = new(10, 31, 0, 24, 8, 7);
    TestEffortValues effortValues = new(
      Vitality: 18,
      Stamina: 54,
      Attack: 54,
      Defense: 18,
      SpecialAttack: 126,
      SpecialDefense: 216,
      Speed: 252);

    PokemonStatistics statistics = new(baseStatistics, individualValues, effortValues, level: 50, PokemonNatures.Adamant);

    Assert.Equal(112, statistics.Vitality);
    Assert.Equal(116, statistics.Stamina);
    Assert.Equal(83, statistics.Attack);
    Assert.Equal(56, statistics.Defense);
    Assert.Equal(87, statistics.SpecialAttack);
    Assert.Equal(101, statistics.SpecialDefense);
    Assert.Equal(85, statistics.Speed);
  }

  private sealed record TestEffortValues(
    byte Vitality = 0,
    byte Stamina = 0,
    byte Attack = 0,
    byte Defense = 0,
    byte SpecialAttack = 0,
    byte SpecialDefense = 0,
    byte Speed = 0) : IEffortValues;
}
