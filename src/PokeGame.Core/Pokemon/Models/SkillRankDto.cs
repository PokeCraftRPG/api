namespace PokeGame.Core.Pokemon.Models;

public record SkillRankDto
{
  public PokemonSkill Skill { get; set; }
  public byte Rank { get; set; }

  public SkillRankDto()
  {
  }

  public SkillRankDto(PokemonSkill skill, byte rank)
  {
    Skill = skill;
    Rank = rank;
  }
}
