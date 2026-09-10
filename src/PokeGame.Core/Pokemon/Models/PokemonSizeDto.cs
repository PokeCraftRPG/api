namespace PokeGame.Core.Pokemon.Models;

public record PokemonSizeDto : IPokemonSize
{
  public byte Scale { get; set; }
  public SizeCategory Category { get; set; }

  public PokemonSizeDto()
  {
  }

  public PokemonSizeDto(byte scale)
  {
    Scale = scale;
    Category = PokemonSize.CalculateCategory(scale);
  }
}
