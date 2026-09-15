using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Infrastructure;
using PokeGame.Infrastructure.Entities;

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

  [Fact(DisplayName = "It should calculate a skill from training and an attribute.")]
  public void Given_TrainingAndAttribute_When_CalculateSkill_Then_SkillBuilt()
  {
    PokemonAttributeDto attribute = new() { Base = 2, Individual = 1, Nature = 1 };
    PokemonSkillDto skill = PokemonMapper.CalculateSkill(new PokemonSkillTraining(1, 4), attribute, null!);

    Assert.Equal(1, skill.Training);
    Assert.Equal(4, skill.Rank);
    Assert.Equal(4, skill.Attribute);
    Assert.Equal(0, skill.Modifiers);
    Assert.Equal(4 + 4, skill.Total); // effective rank (1+2+1) + attribute
  }

  [Fact(DisplayName = "It should treat a missing attribute as zero for Performance.")]
  public void Given_NullAttribute_When_CalculateSkill_Then_AttributeZero()
  {
    PokemonSkillDto skill = PokemonMapper.CalculateSkill(new PokemonSkillTraining(2, 5), attribute: null, null!);

    Assert.Equal(2, skill.Training);
    Assert.Equal(5, skill.Rank);
    Assert.Equal(0, skill.Attribute);
    Assert.Equal(0, skill.Modifiers);
    Assert.Equal(7, skill.Total);
  }

  [Fact(DisplayName = "It should map attributes to Pokémon skills.")]
  public void Given_Attributes_When_CalculateSkills_Then_Mapped()
  {
    PokemonAttributesDto attributes = new()
    {
      Vigor = new() { Base = 1, Individual = 1, Nature = 1 },
      Fortitude = new() { Base = 0, Individual = 0, Nature = 0 },
      Mind = new() { Base = 2, Individual = 1, Nature = -1 },
      Spirit = new() { Base = 2, Individual = 0, Nature = 0 },
      Dexterity = new() { Base = 0, Individual = -1, Nature = 0 }
    };
    Dictionary<PokemonSkill, PokemonSkillTraining> training = new()
    {
      [PokemonSkill.Acrobatics] = new(0, 2),
      [PokemonSkill.Athletics] = new(1, 2),
      [PokemonSkill.Discipline] = new(2, 5),
      [PokemonSkill.Melee] = new(1, 0),
      [PokemonSkill.Occultism] = new(0, 0),
      [PokemonSkill.Perception] = new(3, 9),
      [PokemonSkill.Performance] = new(4, 10),
      [PokemonSkill.Resistance] = new(0, 1),
      [PokemonSkill.Stealth] = new(1, 4),
      [PokemonSkill.Survival] = new(2, 7)
    };

    PokemonSkillsDto skills = PokemonMapper.CalculateSkills(null!, training, attributes);

    AssertSkill(skills.Acrobatics, training: 0, rank: 2, attribute: attributes.Dexterity.Total);
    AssertSkill(skills.Athletics, training: 1, rank: 2, attribute: attributes.Vigor.Total);
    AssertSkill(skills.Discipline, training: 2, rank: 5, attribute: attributes.Spirit.Total);
    AssertSkill(skills.Melee, training: 1, rank: 0, attribute: attributes.Vigor.Total);
    AssertSkill(skills.Occultism, training: 0, rank: 0, attribute: attributes.Mind.Total);
    AssertSkill(skills.Perception, training: 3, rank: 9, attribute: attributes.Spirit.Total);
    AssertSkill(skills.Performance, training: 4, rank: 10, attribute: 0);
    AssertSkill(skills.Resistance, training: 0, rank: 1, attribute: attributes.Fortitude.Total);
    AssertSkill(skills.Stealth, training: 1, rank: 4, attribute: attributes.Dexterity.Total);
    AssertSkill(skills.Survival, training: 2, rank: 7, attribute: attributes.Fortitude.Total);
  }

  [Fact(DisplayName = "It should default missing skill training to zero.")]
  public void Given_MissingTraining_When_CalculateSkills_Then_Defaulted()
  {
    PokemonAttributesDto attributes = new()
    {
      Vigor = new() { Base = 1 },
      Fortitude = new() { Base = 2 },
      Mind = new() { Base = 3 },
      Spirit = new() { Base = 4 },
      Dexterity = new() { Base = 5 }
    };

    PokemonSkillsDto skills = PokemonMapper.CalculateSkills(
      null!,
      new Dictionary<PokemonSkill, PokemonSkillTraining>(),
      attributes);

    AssertSkill(skills.Acrobatics, training: 0, rank: 0, attribute: 5);
    AssertSkill(skills.Athletics, training: 0, rank: 0, attribute: 1);
    AssertSkill(skills.Discipline, training: 0, rank: 0, attribute: 4);
    AssertSkill(skills.Melee, training: 0, rank: 0, attribute: 1);
    AssertSkill(skills.Occultism, training: 0, rank: 0, attribute: 3);
    AssertSkill(skills.Perception, training: 0, rank: 0, attribute: 4);
    AssertSkill(skills.Performance, training: 0, rank: 0, attribute: 0);
    AssertSkill(skills.Resistance, training: 0, rank: 0, attribute: 2);
    AssertSkill(skills.Stealth, training: 0, rank: 0, attribute: 5);
    AssertSkill(skills.Survival, training: 0, rank: 0, attribute: 2);
  }

  [Fact(DisplayName = "It should calculate Pokémon statistics from base, individual, effort and nature.")]
  public void Given_Inputs_When_CalculateStatistics_Then_Mapped()
  {
    BaseStatistics baseStatistics = new(45, 49, 49, 65, 65, 45);
    IndividualValues individualValues = new(10, 31, 0, 24, 8, 7);
    PokemonEntity pokemon = CreatePokemonEntity(level: 50);

    PokemonStatisticsDto statistics = pokemon.CalculateStatistics(
      baseStatistics,
      individualValues,
      new Dictionary<PokemonSkill, PokemonSkillTraining>(),
      PokemonNatures.Adamant);

    PokemonStatistics expected = new(baseStatistics, individualValues, new EffortValues(), level: 50, PokemonNatures.Adamant);

    AssertStatistic(statistics.Vitality, baseStatistics.HP, individualValues.HP, effort: 0, expected.Vitality);
    AssertStatistic(statistics.Stamina, baseStatistics.HP, individualValues.HP, effort: 0, expected.Stamina);
    AssertStatistic(statistics.Attack, baseStatistics.Attack, individualValues.Attack, effort: 0, expected.Attack);
    AssertStatistic(statistics.Defense, baseStatistics.Defense, individualValues.Defense, effort: 0, expected.Defense);
    AssertStatistic(statistics.SpecialAttack, baseStatistics.SpecialAttack, individualValues.SpecialAttack, effort: 0, expected.SpecialAttack);
    AssertStatistic(statistics.SpecialDefense, baseStatistics.SpecialDefense, individualValues.SpecialDefense, effort: 0, expected.SpecialDefense);
    AssertStatistic(statistics.Speed, baseStatistics.Speed, individualValues.Speed, effort: 0, expected.Speed);
  }

  [Fact(DisplayName = "It should include effort values from skill training in statistics.")]
  public void Given_SkillTraining_When_CalculateStatistics_Then_EffortApplied()
  {
    BaseStatistics baseStatistics = new(45, 49, 49, 65, 65, 45);
    IndividualValues individualValues = new(0, 0, 0, 0, 0, 0);
    Dictionary<PokemonSkill, PokemonSkillTraining> skills = new()
    {
      [PokemonSkill.Survival] = new(4, 10),   // EV 252 → Vitality
      [PokemonSkill.Athletics] = new(0, 2),   // EV 18 → Stamina
      [PokemonSkill.Melee] = new(1, 2)        // EV 54 → Attack
    };
    PokemonEntity pokemon = CreatePokemonEntity(level: 50);

    PokemonStatisticsDto statistics = pokemon.CalculateStatistics(
      baseStatistics,
      individualValues,
      skills,
      PokemonNatures.Hardy);

    Assert.Equal((byte)252, statistics.Vitality.Effort);
    Assert.Equal((byte)18, statistics.Stamina.Effort);
    Assert.Equal((byte)54, statistics.Attack.Effort);
    Assert.True(statistics.Vitality.Total > statistics.Stamina.Total);
  }

  private static PokemonEntity CreatePokemonEntity(int level)
  {
    PokemonEntity pokemon = (PokemonEntity)Activator.CreateInstance(typeof(PokemonEntity), nonPublic: true)!;
    typeof(PokemonEntity).GetProperty(nameof(PokemonEntity.Level))!.SetValue(pokemon, level);
    return pokemon;
  }

  private static void AssertAttribute(PokemonAttributeDto attribute, int expectedBase, int expectedIndividual, int expectedNature)
  {
    Assert.Equal(expectedBase, attribute.Base);
    Assert.Equal(expectedIndividual, attribute.Individual);
    Assert.Equal(expectedNature, attribute.Nature);
    Assert.Equal(0, attribute.Modifiers);
    Assert.Equal(expectedBase + expectedIndividual + expectedNature, attribute.Total);
  }

  private static void AssertSkill(PokemonSkillDto skill, int training, int rank, int attribute)
  {
    Assert.Equal(training, skill.Training);
    Assert.Equal(rank, skill.Rank);
    Assert.Equal(attribute, skill.Attribute);
    Assert.Equal(0, skill.Modifiers);
    Assert.Equal(new PokemonSkillTraining(training, rank).GetEffectiveRank() + attribute, skill.Total);
  }

  private static void AssertStatistic(PokemonStatisticDto statistic, byte @base, byte individual, byte effort, int total)
  {
    Assert.Equal(@base, statistic.Base);
    Assert.Equal(individual, statistic.Individual);
    Assert.Equal(effort, statistic.Effort);
    Assert.Equal(total, statistic.Total);
  }
}
