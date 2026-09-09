namespace PokeGame.Core.Species.Models;

public record SpeciesEggsDto : ISpeciesEggs
{
  public byte Cycles { get; set; }
  public EggGroup PrimaryGroup { get; set; }
  public EggGroup? SecondaryGroup { get; set; }
}
