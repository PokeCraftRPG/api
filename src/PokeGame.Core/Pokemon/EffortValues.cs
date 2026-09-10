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

  public EffortValues(IReadOnlyDictionary<PokemonSkill, byte> skillRanks)
  {
    HP = 0; // TODO(fpion): implement
    Attack = CalculateEffortValue(skillRanks.GetValueOrDefault(PokemonSkill.Melee));
    Defense = CalculateEffortValue(skillRanks.GetValueOrDefault(PokemonSkill.Resistance));
    SpecialAttack = CalculateEffortValue(skillRanks.GetValueOrDefault(PokemonSkill.Occultism));
    SpecialDefense = CalculateEffortValue(skillRanks.GetValueOrDefault(PokemonSkill.Discipline));
    Speed = CalculateEffortValue(skillRanks.GetValueOrDefault(PokemonSkill.Acrobatics));
  }

  private static byte CalculateEffortValue(byte rank) => (byte)(Math.Max(rank, MaximumSkillRank) * EffortValuePerSkillRank);
}
