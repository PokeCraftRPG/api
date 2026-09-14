namespace PokeGame.Core.Pokemon.Models;

public record PokemonAttributesDto
{
  public PokemonAttributeDto Dexterity { get; set; } = new();
  public PokemonAttributeDto Fortitude { get; set; } = new();
  public PokemonAttributeDto Mind { get; set; } = new();
  public PokemonAttributeDto Spirit { get; set; } = new();
  public PokemonAttributeDto Vigor { get; set; } = new();
}
