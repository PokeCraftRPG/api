namespace PokeGame.Core.Pokemon;

public sealed record EffortValues
{
  private const byte MaximumSkillRank = 14;
  private const byte EffortValuePerSkillRank = 18;

  public byte HP { get; }
  public byte Attack { get; }
  public byte Defense { get; }
  public byte SpecialAttack { get; }
  public byte SpecialDefense { get; }
  public byte Speed { get; }

  public EffortValues(IReadOnlyDictionary<PokemonSkill, PokemonSkillTraining> skillTraining)
  {
    HP = 0; // TODO(fpion): implement
    Attack = CalculateEffortValue(skillTraining.GetValueOrDefault(PokemonSkill.Melee)?.GetEffectiveRank() ?? 0);
    Defense = CalculateEffortValue(skillTraining.GetValueOrDefault(PokemonSkill.Resistance)?.GetEffectiveRank() ?? 0);
    SpecialAttack = CalculateEffortValue(skillTraining.GetValueOrDefault(PokemonSkill.Occultism)?.GetEffectiveRank() ?? 0);
    SpecialDefense = CalculateEffortValue(skillTraining.GetValueOrDefault(PokemonSkill.Discipline)?.GetEffectiveRank() ?? 0);
    Speed = CalculateEffortValue(skillTraining.GetValueOrDefault(PokemonSkill.Acrobatics)?.GetEffectiveRank() ?? 0);
  }

  private static byte CalculateEffortValue(int rank) => (byte)(Math.Min(rank, MaximumSkillRank) * EffortValuePerSkillRank);
}
