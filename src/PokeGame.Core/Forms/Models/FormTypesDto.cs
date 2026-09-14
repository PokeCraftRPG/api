namespace PokeGame.Core.Forms.Models;

public record FormTypesDto : IFormTypes
{
  public PokemonType Primary { get; set; }
  public PokemonType? Secondary { get; set; }

  public FormTypesDto()
  {
  }

  public FormTypesDto(PokemonType primary, PokemonType? secondary = null)
  {
    Primary = primary;
    Secondary = secondary;
  }

  public FormTypesDto(IFormTypes types) : this(types.Primary, types.Secondary)
  {
  }
}
