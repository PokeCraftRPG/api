using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed record PokemonStatistics
{
  public int Vitality { get; }
  public int Stamina { get; }
  public int Attack { get; }
  public int Defense { get; }
  public int SpecialAttack { get; }
  public int SpecialDefense { get; }
  public int Speed { get; }

  public PokemonStatistics(IBaseStatistics baseStatistics, IIndividualValues individualValues, IEffortValues effortValues, int level, PokemonNature nature)
  {
    Vitality = CalculateHP(baseStatistics.HP, individualValues.HP, effortValues.Vitality, level);
    Stamina = CalculateHP(baseStatistics.HP, individualValues.HP, effortValues.Stamina, level);
    Attack = CalculateOther(baseStatistics.Attack, individualValues.Attack, effortValues.Attack, level, nature.GetMultiplier(PokemonStatistic.Attack));
    Defense = CalculateOther(baseStatistics.Defense, individualValues.Defense, effortValues.Defense, level, nature.GetMultiplier(PokemonStatistic.Defense));
    SpecialAttack = CalculateOther(baseStatistics.SpecialAttack, individualValues.SpecialAttack, effortValues.SpecialAttack, level, nature.GetMultiplier(PokemonStatistic.SpecialAttack));
    SpecialDefense = CalculateOther(baseStatistics.SpecialDefense, individualValues.SpecialDefense, effortValues.SpecialDefense, level, nature.GetMultiplier(PokemonStatistic.SpecialDefense));
    Speed = CalculateOther(baseStatistics.Speed, individualValues.Speed, effortValues.Speed, level, nature.GetMultiplier(PokemonStatistic.Speed));
  }

  private static int CalculateHP(int @base, byte individualValue, byte effortValue, int level)
  {
    return (int)Math.Floor(((2.0 * @base) + individualValue + Math.Floor(effortValue / 4.0)) * level / 100.0) + level + 10;
  }
  private static int CalculateOther(int @base, byte individualValue, byte effortValue, int level, double nature)
  {
    return (int)Math.Floor((Math.Floor(((2.0 * @base) + individualValue + Math.Floor(effortValue / 4.0)) * level / 100.0) + 5) * nature);
  }
}
