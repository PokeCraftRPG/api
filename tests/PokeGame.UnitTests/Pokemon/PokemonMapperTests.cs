using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Infrastructure;

namespace PokeGame.Pokemon;

public class PokemonMapperTests : UnitTests
{
  [Theory(DisplayName = "It should calculate the attribute base from a base statistic.")]
  [InlineData(0, -3)]
  [InlineData(14, -3)]
  [InlineData(15, -2)]
  [InlineData(44, -1)]
  [InlineData(45, 0)]
  [InlineData(54, 0)]
  [InlineData(55, 1)]
  [InlineData(104, 5)]
  [InlineData(105, 6)]
  [InlineData(124, 6)]
  [InlineData(125, 7)]
  [InlineData(144, 7)]
  [InlineData(145, 8)]
  [InlineData(255, 8)]
  public void Given_BaseStatistic_When_CalculateAttributeBase_Then_Expected(byte baseValue, int expected)
  {
    Assert.Equal(expected, PokemonMapper.CalculateAttributeBase(baseValue));
  }

  [Theory(DisplayName = "It should calculate the attribute individual from an individual value.")]
  [InlineData(0, -1)]
  [InlineData(7, -1)]
  [InlineData(8, 0)]
  [InlineData(23, 0)]
  [InlineData(24, 1)]
  [InlineData(31, 1)]
  public void Given_IndividualValue_When_CalculateAttributeIndividual_Then_Expected(byte individualValue, int expected)
  {
    Assert.Equal(expected, PokemonMapper.CalculateAttributeIndividual(individualValue));
  }

  [Fact(DisplayName = "It should return +1 when the nature increases the statistic.")]
  public void Given_IncreasedStatistic_When_CalculateAttributeNature_Then_PlusOne()
  {
    Assert.Equal(1, PokemonMapper.CalculateAttributeNature(PokemonNatures.Adamant, PokemonStatistic.Attack));
  }

  [Fact(DisplayName = "It should return -1 when the nature decreases the statistic.")]
  public void Given_DecreasedStatistic_When_CalculateAttributeNature_Then_MinusOne()
  {
    Assert.Equal(-1, PokemonMapper.CalculateAttributeNature(PokemonNatures.Adamant, PokemonStatistic.SpecialAttack));
  }

  [Fact(DisplayName = "It should return 0 when the nature does not affect the statistic.")]
  public void Given_NeutralStatistic_When_CalculateAttributeNature_Then_Zero()
  {
    Assert.Equal(0, PokemonMapper.CalculateAttributeNature(PokemonNatures.Adamant, PokemonStatistic.Speed));
    Assert.Equal(0, PokemonMapper.CalculateAttributeNature(PokemonNatures.Hardy, PokemonStatistic.Attack));
  }

  [Fact(DisplayName = "It should calculate a full attribute from base, individual and nature.")]
  public void Given_Inputs_When_CalculateAttribute_Then_AttributeBuilt()
  {
    PokemonAttributeDto attribute = PokemonMapper.CalculateAttribute(
      baseValue: 65,
      individualValue: 31,
      PokemonNatures.Modest,
      PokemonStatistic.SpecialAttack,
      null!);

    Assert.Equal(2, attribute.Base);
    Assert.Equal(1, attribute.Individual);
    Assert.Equal(1, attribute.Nature);
    Assert.Equal(0, attribute.Modifiers);
    Assert.Equal(4, attribute.Total);
  }

  [Fact(DisplayName = "It should map battle statistics to Pokémon attributes.")]
  public void Given_Statistics_When_CalculateAttributes_Then_Mapped()
  {
    BaseStatistics baseStatistics = new(45, 49, 49, 65, 65, 45);
    IndividualValues individualValues = new(10, 31, 0, 24, 8, 7);
    PokemonAttributesDto attributes = PokemonMapper.CalculateAttributes(
      null!,
      baseStatistics,
      individualValues,
      PokemonNatures.Adamant);

    AssertAttribute(attributes.Vigor, expectedBase: 0, expectedIndividual: 1, expectedNature: 1);
    AssertAttribute(attributes.Fortitude, expectedBase: 0, expectedIndividual: -1, expectedNature: 0);
    AssertAttribute(attributes.Mind, expectedBase: 2, expectedIndividual: 1, expectedNature: -1);
    AssertAttribute(attributes.Spirit, expectedBase: 2, expectedIndividual: 0, expectedNature: 0);
    AssertAttribute(attributes.Dexterity, expectedBase: 0, expectedIndividual: -1, expectedNature: 0);
  }

  private static void AssertAttribute(PokemonAttributeDto attribute, int expectedBase, int expectedIndividual, int expectedNature)
  {
    Assert.Equal(expectedBase, attribute.Base);
    Assert.Equal(expectedIndividual, attribute.Individual);
    Assert.Equal(expectedNature, attribute.Nature);
    Assert.Equal(0, attribute.Modifiers);
    Assert.Equal(expectedBase + expectedIndividual + expectedNature, attribute.Total);
  }
}
