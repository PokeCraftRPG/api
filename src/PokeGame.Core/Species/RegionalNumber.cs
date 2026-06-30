using PokeGame.Core.Regions;

namespace PokeGame.Core.Species;

public class RegionalNumber
{
  public PokemonSpecies? Species { get; private set; }
  public int SpeciesId { get; private set; }

  public Region? Region { get; private set; }
  public int RegionId { get; private set; }

  public int Number { get; private set; }

  public RegionalNumber(PokemonSpecies species, Region region, int number)
  {
    Species = species;
    SpeciesId = species.SpeciesId;

    Region = region;
    RegionId = region.RegionId;

    Update(number);
  }

  private RegionalNumber()
  {
  }

  public void Update(int number)
  {
    Number = number;
  }

  public override bool Equals(object? obj) => obj is RegionalNumber regionalNumber && regionalNumber.SpeciesId == SpeciesId && regionalNumber.RegionId == RegionId;
  public override int GetHashCode() => HashCode.Combine(SpeciesId, RegionId);
  public override string ToString() => $"{base.ToString()} (SpeciesId={SpeciesId}, RegionId={RegionId})";
}
