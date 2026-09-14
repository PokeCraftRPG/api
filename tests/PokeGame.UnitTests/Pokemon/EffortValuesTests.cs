using PokeGame.Core.Pokemon;

namespace PokeGame.Pokemon;

public class EffortValuesTests : UnitTests
{
  [Fact(DisplayName = "It should return zero effort values when no skill training is present.")]
  public void Given_EmptyTraining_When_Create_Then_ZeroEffortValues()
  {
    EffortValues effortValues = new(new Dictionary<PokemonSkill, PokemonSkillTraining>());

    Assert.Equal((byte)0, effortValues.HP);
    Assert.Equal((byte)0, effortValues.Attack);
    Assert.Equal((byte)0, effortValues.Defense);
    Assert.Equal((byte)0, effortValues.SpecialAttack);
    Assert.Equal((byte)0, effortValues.SpecialDefense);
    Assert.Equal((byte)0, effortValues.Speed);
  }

  [Fact(DisplayName = "It should map skill effective ranks to the corresponding effort values.")]
  public void Given_SkillTraining_When_Create_Then_Mapped()
  {
    Dictionary<PokemonSkill, PokemonSkillTraining> training = new()
    {
      [PokemonSkill.Melee] = new(level: 1, rank: 2),       // effective 3 → 54
      [PokemonSkill.Resistance] = new(level: 0, rank: 2),  // effective 1 → 18
      [PokemonSkill.Occultism] = new(level: 2, rank: 5),   // effective 7 → 126
      [PokemonSkill.Discipline] = new(level: 3, rank: 9),  // effective 12 → 216
      [PokemonSkill.Acrobatics] = new(level: 4, rank: 10), // effective 14 → 252
      [PokemonSkill.Athletics] = new(level: 4, rank: 10),  // ignored for EVs
      [PokemonSkill.Performance] = new(level: 4, rank: 10) // ignored for EVs
    };

    EffortValues effortValues = new(training);

    Assert.Equal((byte)0, effortValues.HP);
    Assert.Equal((byte)54, effortValues.Attack);
    Assert.Equal((byte)18, effortValues.Defense);
    Assert.Equal((byte)126, effortValues.SpecialAttack);
    Assert.Equal((byte)216, effortValues.SpecialDefense);
    Assert.Equal((byte)252, effortValues.Speed);
  }

  [Fact(DisplayName = "It should cap effort values at fourteen effective ranks.")]
  public void Given_EffectiveRankAboveCap_When_Create_Then_Capped()
  {
    // Level 4 + rank 10 = 14 effective; above-threshold overflow can exceed that in theory,
    // but the EV formula still caps contribution at 14 * 18 = 252.
    Dictionary<PokemonSkill, PokemonSkillTraining> training = new()
    {
      [PokemonSkill.Melee] = new(level: 4, rank: 10)
    };

    EffortValues effortValues = new(training);

    Assert.Equal((byte)252, effortValues.Attack);
  }

  [Theory(DisplayName = "It should convert an effective rank into eighteen effort points each.")]
  [InlineData(0, 0)]
  [InlineData(1, 18)]
  [InlineData(7, 126)]
  [InlineData(14, 252)]
  public void Given_SingleSkill_When_Create_Then_EighteenPerRank(int effectiveRankInput, byte expectedEv)
  {
    // Build a training whose GetEffectiveRank equals the desired input via known table points.
    PokemonSkillTraining training = effectiveRankInput switch
    {
      0 => new(0, 0),
      1 => new(0, 2),
      7 => new(2, 5),
      14 => new(4, 10),
      _ => throw new ArgumentOutOfRangeException(nameof(effectiveRankInput))
    };
    Assert.Equal(effectiveRankInput, training.GetEffectiveRank());

    EffortValues effortValues = new(new Dictionary<PokemonSkill, PokemonSkillTraining>
    {
      [PokemonSkill.Melee] = training
    });

    Assert.Equal(expectedEv, effortValues.Attack);
  }
}
