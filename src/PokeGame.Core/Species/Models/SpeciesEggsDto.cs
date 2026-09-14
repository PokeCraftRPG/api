namespace PokeGame.Core.Species.Models;

public record SpeciesEggsDto : ISpeciesEggs
{
  public byte Cycles { get; set; }
  public EggGroup PrimaryGroup { get; set; }
  public EggGroup? SecondaryGroup { get; set; }

  public SpeciesEggsDto()
  {
  }

  public SpeciesEggsDto(byte cycles, EggGroup primaryGroup, EggGroup? secondaryGroup = null)
  {
    Cycles = cycles;
    PrimaryGroup = primaryGroup;
    SecondaryGroup = secondaryGroup;
  }

  public SpeciesEggsDto(ISpeciesEggs eggs) : this(eggs.Cycles, eggs.PrimaryGroup, eggs.SecondaryGroup)
  {
  }
}
