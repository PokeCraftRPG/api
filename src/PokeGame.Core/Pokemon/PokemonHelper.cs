using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public static class PokemonHelper
{
  public static int CalculateMaximumStamina(
    this Specimen specimen,
    IBaseStatistics? baseStatistics = null,
    IIndividualValues? individualValues = null,
    IEffortValues? effortValues = null,
    int? level = null)
  {
    baseStatistics ??= specimen.BaseStatistics;
    individualValues ??= specimen.IndividualValues;
    effortValues ??= new EffortValues(specimen.Skills);
    level ??= specimen.Level;
    return CalculateMaximumStamina(baseStatistics, individualValues, effortValues, level.Value);
  }
  public static int CalculateMaximumStamina(IBaseStatistics baseStatistics, IIndividualValues individualValues, IEffortValues effortValues, int level)
  {
    return CalculateHP(baseStatistics.HP, individualValues.HP, effortValues.Stamina, level);
  }

  public static int CalculateMaximumVitality(
    this Specimen specimen,
    IBaseStatistics? baseStatistics = null,
    IIndividualValues? individualValues = null,
    IEffortValues? effortValues = null,
    int? level = null)
  {
    baseStatistics ??= specimen.BaseStatistics;
    individualValues ??= specimen.IndividualValues;
    effortValues ??= new EffortValues(specimen.Skills);
    level ??= specimen.Level;
    return CalculateMaximumVitality(baseStatistics, individualValues, effortValues, level.Value);
  }
  public static int CalculateMaximumVitality(IBaseStatistics baseStatistics, IIndividualValues individualValues, IEffortValues effortValues, int level)
  {
    return CalculateHP(baseStatistics.HP, individualValues.HP, effortValues.Vitality, level);
  }

  private static int CalculateHP(byte @base, byte individualValue, byte effortValue, int level)
  {
    return (int)Math.Floor(((2.0 * @base) + individualValue + Math.Floor(effortValue / 4.0)) * level / 100.0) + level + 10;
  }
}
