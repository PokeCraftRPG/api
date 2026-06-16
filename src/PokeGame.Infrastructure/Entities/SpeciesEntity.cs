using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;

namespace PokeGame.Infrastructure.Entities;

internal class SpeciesEntity : AggregateEntity
{
  public int SpeciesId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid EntityId { get; private set; }

  public int Number { get; private set; }
  public PokemonCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public byte BaseFriendship { get; private set; }
  public byte CatchRate { get; private set; }
  public GrowthRate GrowthRate { get; private set; }

  public byte EggCycles { get; private set; }
  public EggGroup PrimaryEggGroup { get; private set; }
  public EggGroup? SecondaryEggGroup { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public SpeciesEntity(WorldEntity world, SpeciesCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    EntityId = new SpeciesId(@event.StreamId).EntityId;

    Number = @event.Number.Value;
    Category = @event.Category;

    Key = @event.Key.Value;

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;
    EggCycles = @event.EggCycles.Value;
    PrimaryEggGroup = @event.EggGroups.Primary;
    SecondaryEggGroup = @event.EggGroups.Secondary;
  }

  private SpeciesEntity() : base()
  {
  }

  public void Describe(SpeciesDescribed @event)
  {
    base.Update(@event);

    Description = @event.Description?.Value;
  }

  public RegionalNumberEntity? RemoveRegionalNumber(SpeciesRegionalNumberRemoved @event)
  {
    base.Update(@event);

    return RegionalNumbers.SingleOrDefault(x => x.Region?.StreamId == @event.StreamId.Value);
  }

  public void Rename(SpeciesRenamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void SetGameData(SpeciesGameDataChanged @event)
  {
    base.Update(@event);

    BaseFriendship = @event.BaseFriendship.Value;
    CatchRate = @event.CatchRate.Value;
    GrowthRate = @event.GrowthRate;
    EggCycles = @event.EggCycles.Value;
    PrimaryEggGroup = @event.EggGroups.Primary;
    SecondaryEggGroup = @event.EggGroups.Secondary;
  }

  public void SetKey(SpeciesKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void SetRegionalNumber(SpeciesRegionalNumberChanged @event, RegionEntity region)
  {
    base.Update(@event);

    RegionalNumberEntity? regionalNumber = RegionalNumbers.SingleOrDefault(x => x.RegionId == region.RegionId);
    if (regionalNumber is null)
    {
      regionalNumber = new RegionalNumberEntity(this, region, @event);
      RegionalNumbers.Add(regionalNumber);
    }
    else
    {
      regionalNumber.Update(@event);
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
