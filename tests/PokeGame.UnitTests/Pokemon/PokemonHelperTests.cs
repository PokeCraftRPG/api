using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;

namespace PokeGame.Pokemon;

public class PokemonHelperTests : UnitTests
{
  [Theory(DisplayName = "It should calculate maximum vitality with the HP formula.")]
  [InlineData(45, 0, 0, 1, 11)]
  [InlineData(45, 31, 0, 50, 120)]
  [InlineData(45, 0, 252, 50, 136)]
  public void Given_Inputs_When_CalculateMaximumVitality_Then_Expected(
    byte @base,
    byte individual,
    byte vitalityEffort,
    int level,
    int expected)
  {
    int actual = PokemonHelper.CalculateMaximumVitality(
      new BaseStatistics(@base, 10, 10, 10, 10, 10),
      new IndividualValues(individual, 0, 0, 0, 0, 0),
      new TestEffortValues(Vitality: vitalityEffort),
      level);

    Assert.Equal(expected, actual);
  }

  [Theory(DisplayName = "It should calculate maximum stamina with the HP formula and stamina effort.")]
  [InlineData(45, 0, 0, 1, 11)]
  [InlineData(45, 31, 0, 50, 120)]
  [InlineData(45, 0, 252, 50, 136)]
  public void Given_Inputs_When_CalculateMaximumStamina_Then_Expected(
    byte @base,
    byte individual,
    byte staminaEffort,
    int level,
    int expected)
  {
    int actual = PokemonHelper.CalculateMaximumStamina(
      new BaseStatistics(@base, 10, 10, 10, 10, 10),
      new IndividualValues(individual, 0, 0, 0, 0, 0),
      new TestEffortValues(Stamina: staminaEffort),
      level);

    Assert.Equal(expected, actual);
  }

  [Fact(DisplayName = "It should use specimen defaults when calculating maximum vitality and stamina.")]
  public void Given_Specimen_When_CalculateMaximum_Then_UsesSpecimenValues()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    Assert.Equal(
      PokemonHelper.CalculateMaximumVitality(pokemon.BaseStatistics, pokemon.IndividualValues, new EffortValues(pokemon.Skills), pokemon.Level),
      pokemon.CalculateMaximumVitality());
    Assert.Equal(
      PokemonHelper.CalculateMaximumStamina(pokemon.BaseStatistics, pokemon.IndividualValues, new EffortValues(pokemon.Skills), pokemon.Level),
      pokemon.CalculateMaximumStamina());
    Assert.Equal(pokemon.CalculateMaximumVitality(), pokemon.Vitality);
    Assert.Equal(pokemon.CalculateMaximumStamina(), pokemon.Stamina);
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
