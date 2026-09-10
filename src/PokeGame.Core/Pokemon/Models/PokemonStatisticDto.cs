namespace PokeGame.Core.Pokemon.Models;

public record PokemonStatisticDto
{
  public byte Base { get; set; }
  public byte Individual { get; set; }
  public byte Effort { get; set; }
  public int Total { get; set; }

  public PokemonStatisticDto()
  {
  }

  public PokemonStatisticDto(byte @base, byte individual, byte effort, int total)
  {
    Base = @base;
    Individual = individual;
    Effort = effort;
    Total = total;
  }
}
