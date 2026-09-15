namespace PokeGame.Core.Pokemon;

public interface IEffortValues
{
  byte Vitality { get; }
  byte Stamina { get; }
  byte Attack { get; }
  byte Defense { get; }
  byte SpecialAttack { get; }
  byte SpecialDefense { get; }
  byte Speed { get; }
}

public sealed record EffortValues : IEffortValues
{
  public byte Vitality { get; }
  public byte Stamina { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public EffortValues()
  {
  }

  public EffortValues(IReadOnlyDictionary<PokemonSkill, PokemonSkillTraining> skillTraining)
  {
    Vitality = CalculateEffortValue(skillTraining, PokemonSkill.Survival);
    Stamina = CalculateEffortValue(skillTraining, PokemonSkill.Athletics);
    Attack = CalculateEffortValue(skillTraining, PokemonSkill.Melee);
    Defense = CalculateEffortValue(skillTraining, PokemonSkill.Resistance);
    SpecialAttack = CalculateEffortValue(skillTraining, PokemonSkill.Occultism);
    SpecialDefense = CalculateEffortValue(skillTraining, PokemonSkill.Discipline);
    Speed = CalculateEffortValue(skillTraining, PokemonSkill.Acrobatics);
  }

  private static byte CalculateEffortValue(IReadOnlyDictionary<PokemonSkill, PokemonSkillTraining> skillTraining, PokemonSkill skill)
  {
    if (skillTraining.TryGetValue(skill, out PokemonSkillTraining? training))
    {
      return (byte)(training.GetEffectiveRank() * 18);
    }
    return 0;
  }
}
