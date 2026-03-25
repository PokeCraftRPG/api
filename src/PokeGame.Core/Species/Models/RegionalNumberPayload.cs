namespace PokeGame.Core.Species.Models;

public record RegionalNumberPayload
{
  public string Region { get; set; }
  public int Number { get; set; }

  public RegionalNumberPayload() : this(string.Empty, default)
  {
  }

  public RegionalNumberPayload(string region, int number)
  {
    Region = region;
    Number = number;
  }
}
