using Logitar;
using PokeGame.Core.Species.Events;

namespace PokeGame.Infrastructure.Entities;

internal class RegionalNumberEntity
{
  public SpeciesEntity? Species { get; private set; }
  public int SpeciesId { get; private set; }

  public RegionEntity? Region { get; private set; }
  public int RegionId { get; private set; }

  public int Number { get; private set; }

  public string? CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }

  public string? UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public RegionalNumberEntity(SpeciesEntity species, int regionId, SpeciesRegionalNumberChanged @event)
  {
    Species = species;
    SpeciesId = species.SpeciesId;

    RegionId = regionId;

    CreatedBy = @event.ActorId?.Value;
    CreatedOn = @event.OccurredOn.AsUniversalTime();

    Update(@event);
  }

  private RegionalNumberEntity()
  {
  }

  public void Update(SpeciesRegionalNumberChanged @event)
  {
    Number = @event.Number.Value;

    UpdatedBy = @event.ActorId?.Value;
    UpdatedOn = @event.OccurredOn.AsUniversalTime();
  }

  public override bool Equals(object? obj) => obj is RegionalNumberEntity entity && entity.SpeciesId == SpeciesId && entity.RegionId == RegionId;
  public override int GetHashCode() => HashCode.Combine(SpeciesId, RegionId);
  public override string ToString() => $"{base.ToString()} (SpeciesId={SpeciesId}, RegionId={RegionId})";
}
