using PokeGame.Core.Species.Events;

namespace PokeGame.Infrastructure.Entities;

internal class RegionalNumberEntity
{
  public SpeciesEntity? Species { get; private set; }
  public int SpeciesId { get; private set; }

  public RegionEntity? Region { get; private set; }
  public int RegionId { get; private set; }

  public int Number { get; set; }

  public RegionalNumberEntity(SpeciesEntity species, RegionEntity region, SpeciesRegionalNumberChanged @event)
  {
    Species = species;
    SpeciesId = species.SpeciesId;

    Region = region;
    RegionId = region.RegionId;

    Update(@event);
  }

  private RegionalNumberEntity()
  {
  }

  public void Update(SpeciesRegionalNumberChanged @event)
  {
    Number = @event.Number.Value;
  }

  public override bool Equals(object? obj) => obj is RegionalNumberEntity regionalNumber && regionalNumber.SpeciesId == SpeciesId && regionalNumber.RegionId == RegionId;
  public override int GetHashCode() => HashCode.Combine(SpeciesId, RegionId);
  public override string ToString() => $"{base.ToString()} (SpeciesId={SpeciesId}, RegionId={RegionId})";
}
