namespace PokeGame.Core.Pokemon.Models;

public record SkillRankDto
{
  public PokemonSkill Skill { get; set; }
  public int Rank { get; set; }

  public SkillRankDto()
  {
  }

  public SkillRankDto(PokemonSkill skill, int rank)
  {
    Skill = skill;
    Rank = rank;
  }
}
