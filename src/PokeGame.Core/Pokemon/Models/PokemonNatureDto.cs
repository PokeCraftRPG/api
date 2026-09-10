namespace PokeGame.Core.Pokemon.Models;

public record PokemonNatureDto : IPokemonNature
{
  public string Name { get; set; } = string.Empty;
  public PokemonStatistic? IncreasedStatistic { get; set; }
  public PokemonStatistic? DecreasedStatistic { get; set; }
  public Flavor? FavoriteFlavor { get; set; }
  public Flavor? DislikedFlavor { get; set; }

  public PokemonNatureDto()
  {
  }

  public PokemonNatureDto(PokemonNature nature)
  {
    Name = nature.Name;
    IncreasedStatistic = nature.IncreasedStatistic;
    DecreasedStatistic = nature.DecreasedStatistic;
    FavoriteFlavor = nature.FavoriteFlavor;
    DislikedFlavor = nature.DislikedFlavor;
  }
}
