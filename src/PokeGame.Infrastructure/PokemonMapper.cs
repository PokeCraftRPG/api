using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Infrastructure.Entities;

namespace PokeGame.Infrastructure;

internal static class PokemonMapper
{
  public static PokemonAttributeDto CalculateAttribute(byte baseValue, byte individualValue, IPokemonNature nature, PokemonStatistic statistic, PokemonEntity _)
  {
    return new PokemonAttributeDto
    {
      Base = CalculateAttributeBase(baseValue),
      Individual = CalculateAttributeIndividual(individualValue),
      Nature = CalculateAttributeNature(nature, statistic),
      Modifiers = 0
    };
  }

  public static int CalculateAttributeBase(byte baseValue)
  {
    if (baseValue < 45)
    {
      return (baseValue / 15) - 3;
    }
    else if (baseValue < 105)
    {
      return (baseValue - 45) / 10;
    }
    else if (baseValue < 125)
    {
      return 6;
    }
    else if (baseValue < 145)
    {
      return 7;
    }
    else
    {
      return 8;
    }
  }

  public static int CalculateAttributeIndividual(byte individualValue) => (individualValue / 8) switch
  {
    0 => -1,
    3 => 1,
    _ => 0,
  };

  public static int CalculateAttributeNature(IPokemonNature nature, PokemonStatistic statistic)
  {
    if (nature.IncreasedStatistic == statistic)
    {
      return 1;
    }
    else if (nature.DecreasedStatistic == statistic)
    {
      return -1;
    }
    else
    {
      return 0;
    }
  }

  public static PokemonAttributesDto CalculateAttributes(
    this PokemonEntity pokemon,
    IBaseStatistics baseStatistics,
    IIndividualValues individualValues,
    IPokemonNature nature)
  {
    return new PokemonAttributesDto
    {
      Dexterity = CalculateAttribute(baseStatistics.Speed, individualValues.Speed, nature, PokemonStatistic.Speed, pokemon),
      Fortitude = CalculateAttribute(baseStatistics.Defense, individualValues.Defense, nature, PokemonStatistic.Defense, pokemon),
      Mind = CalculateAttribute(baseStatistics.SpecialAttack, individualValues.SpecialAttack, nature, PokemonStatistic.SpecialAttack, pokemon),
      Spirit = CalculateAttribute(baseStatistics.SpecialDefense, individualValues.SpecialDefense, nature, PokemonStatistic.SpecialDefense, pokemon),
      Vigor = CalculateAttribute(baseStatistics.Attack, individualValues.Attack, nature, PokemonStatistic.Attack, pokemon)
    };
  }

  public static PokemonSkillDto CalculateSkill(PokemonSkillTraining training, PokemonAttributeDto? attribute, PokemonEntity _)
  {
    return new PokemonSkillDto
    {
      Training = training.Level,
      Rank = training.Rank,
      Attribute = attribute?.Total ?? 0,
      Modifiers = 0
    };
  }

  public static PokemonSkillsDto CalculateSkills(
    this PokemonEntity pokemon,
    IReadOnlyDictionary<PokemonSkill, PokemonSkillTraining> training,
    PokemonAttributesDto attributes)
  {
    return new PokemonSkillsDto
    {
      Acrobatics = CalculateSkill(GetSkillTraining(training, PokemonSkill.Acrobatics), attributes.Dexterity, pokemon),
      Athletics = CalculateSkill(GetSkillTraining(training, PokemonSkill.Athletics), attributes.Vigor, pokemon),
      Discipline = CalculateSkill(GetSkillTraining(training, PokemonSkill.Discipline), attributes.Spirit, pokemon),
      Melee = CalculateSkill(GetSkillTraining(training, PokemonSkill.Melee), attributes.Vigor, pokemon),
      Occultism = CalculateSkill(GetSkillTraining(training, PokemonSkill.Occultism), attributes.Mind, pokemon),
      Perception = CalculateSkill(GetSkillTraining(training, PokemonSkill.Perception), attributes.Spirit, pokemon),
      Performance = CalculateSkill(GetSkillTraining(training, PokemonSkill.Performance), attribute: null, pokemon),
      Resistance = CalculateSkill(GetSkillTraining(training, PokemonSkill.Resistance), attributes.Fortitude, pokemon),
      Stealth = CalculateSkill(GetSkillTraining(training, PokemonSkill.Stealth), attributes.Dexterity, pokemon),
      Survival = CalculateSkill(GetSkillTraining(training, PokemonSkill.Survival), attributes.Fortitude, pokemon)
    };
  }
  private static PokemonSkillTraining GetSkillTraining(IReadOnlyDictionary<PokemonSkill, PokemonSkillTraining> training, PokemonSkill skill)
  {
    return training.TryGetValue(skill, out PokemonSkillTraining? skillTraining) ? skillTraining : new PokemonSkillTraining(level: 0, rank: 0);
  }

  public static BaseStatisticsDto ToBaseStatistics(this PokemonEntity pokemon) => new(
    pokemon.BaseHP,
    pokemon.BaseAttack,
    pokemon.BaseDefense,
    pokemon.BaseSpecialAttack,
    pokemon.BaseSpecialDefense,
    pokemon.BaseSpeed);

  public static IndividualValuesDto ToIndividualValues(this PokemonEntity pokemon) => new(
    pokemon.IndividualHP,
    pokemon.IndividualAttack,
    pokemon.IndividualDefense,
    pokemon.IndividualSpecialAttack,
    pokemon.IndividualSpecialDefense,
    pokemon.IndividualSpeed);
}
