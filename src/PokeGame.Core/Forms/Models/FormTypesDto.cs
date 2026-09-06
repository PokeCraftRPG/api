namespace PokeGame.Core.Forms.Models;

public record FormTypesDto : IFormTypes
{
  public PokemonType Primary { get; set; }
  public PokemonType? Secondary { get; set; }
}
