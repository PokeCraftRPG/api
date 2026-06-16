using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;

namespace PokeGame.Infrastructure.Entities;

internal class RegionEntity : AggregateEntity
{
  public int RegionId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid EntityId { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public RegionEntity(WorldEntity world, RegionCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    EntityId = new RegionId(@event.StreamId).EntityId;

    Key = @event.Key.Value;
  }

  private RegionEntity() : base()
  {
  }

  public void Describe(RegionDescribed @event)
  {
    base.Update(@event);

    Description = @event.Description?.Value;
  }

  public void Rename(RegionRenamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void SetKey(RegionKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
