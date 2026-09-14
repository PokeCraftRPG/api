namespace PokeGame.Core.Pokemon.Models;

public record PokemonSkillsDto
{
  public PokemonSkillDto Acrobatics { get; set; } = new();
  public PokemonSkillDto Athletics { get; set; } = new();
  public PokemonSkillDto Discipline { get; set; } = new();
  public PokemonSkillDto Melee { get; set; } = new();
  public PokemonSkillDto Occultism { get; set; } = new();
  public PokemonSkillDto Perception { get; set; } = new();
  public PokemonSkillDto Performance { get; set; } = new();
  public PokemonSkillDto Resistance { get; set; } = new();
  public PokemonSkillDto Stealth { get; set; } = new();
  public PokemonSkillDto Survival { get; set; } = new();
}
