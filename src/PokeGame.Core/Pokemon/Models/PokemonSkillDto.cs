namespace PokeGame.Core.Pokemon.Models;

public record PokemonSkillDto
{
  public int Training { get; set; }
  public int Rank { get; set; }
  public int Attribute { get; set; }
  public int Modifiers { get; set; }
  public int Total => new PokemonSkillTraining(Training, Rank).GetEffectiveRank() + Attribute + Modifiers;
}
