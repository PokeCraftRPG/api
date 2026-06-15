using Logitar.EventSourcing;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid EntityId { get; private set; }

  public string OwnerId { get; private set; } = string.Empty;

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<AbilityEntity> Abilities { get; private set; } = [];
  public List<MoveEntity> Moves { get; private set; } = [];
  public List<RegionEntity> Regions { get; private set; } = [];

  public WorldEntity(WorldCreated @event) : base(@event)
  {
    EntityId = new WorldId(@event.StreamId).EntityId;

    OwnerId = @event.OwnerId.Value;

    Key = @event.Key.Value;
  }

  private WorldEntity() : base()
  {
  }

  public void Describe(WorldDescribed @event)
  {
    base.Update(@event);

    Description = @event.Description?.Value;
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = base.GetActorIds().ToHashSet();
    actorIds.Add(new ActorId(OwnerId));
    return actorIds;
  }

  public void Rename(WorldRenamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void SetKey(WorldKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
