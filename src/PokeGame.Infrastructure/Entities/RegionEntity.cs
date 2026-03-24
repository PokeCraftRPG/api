using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Events;

namespace PokeGame.Infrastructure.Entities;

internal class RegionEntity : AggregateEntity
{
  public int RegionId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public RegionEntity(WorldEntity world, RegionCreated @event) : base(@event)
  {
    Id = new RegionId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    Key = @event.Key.Value;
  }

  private RegionEntity() : base()
  {
  }

  public void SetKey(RegionKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(RegionUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }

    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
