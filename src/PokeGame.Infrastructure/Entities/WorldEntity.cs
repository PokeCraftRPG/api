using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string OwnerId { get; private set; } = string.Empty;

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<AbilityEntity> Abilities { get; private set; } = [];
  public List<MoveEntity> Moves { get; private set; } = [];
  public List<RegionEntity> Regions { get; private set; } = [];
  public List<SpeciesEntity> Species { get; private set; } = [];

  public WorldEntity(WorldCreated @event) : base(@event)
  {
    Id = new WorldId(@event.StreamId).ToGuid();

    OwnerId = @event.OwnerId.Value;

    Key = @event.Key.Value;
  }

  private WorldEntity() : base()
  {
  }

  public void SetKey(WorldKeyChanged evnet)
  {
    base.Update(evnet);

    Key = evnet.Key.Value;
  }

  public void Update(WorldUpdated @event)
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
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
