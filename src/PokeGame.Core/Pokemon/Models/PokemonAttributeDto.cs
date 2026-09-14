namespace PokeGame.Core.Pokemon.Models;

public record PokemonAttributeDto
{
  public int Base { get; set; }
  public int Individual { get; set; }
  public int Nature { get; set; }
  public int Modifiers { get; set; }
  public int Total => Base + Individual + Nature + Modifiers;
}
