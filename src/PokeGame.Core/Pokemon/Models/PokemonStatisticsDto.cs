namespace PokeGame.Core.Pokemon.Models;

public record PokemonStatisticsDto
{
  public PokemonStatisticDto HP { get; set; } = new();
  public PokemonStatisticDto Attack { get; set; } = new();
  public PokemonStatisticDto Defense { get; set; } = new();
  public PokemonStatisticDto SpecialAttack { get; set; } = new();
  public PokemonStatisticDto SpecialDefense { get; set; } = new();
  public PokemonStatisticDto Speed { get; set; } = new();
}
