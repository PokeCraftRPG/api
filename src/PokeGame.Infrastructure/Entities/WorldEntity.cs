using Logitar.EventSourcing;
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
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public WorldEntity(WorldCreated @event) : base(@event)
  {
    Id = new WorldId(@event.StreamId).EntityId;

    OwnerId = @event.OwnerId.Value;

    Key = @event.Key.Value;
  }

  private WorldEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    actorIds.Add(new ActorId(OwnerId));
    return actorIds;
  }

  public void SetKey(WorldKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(WorldUpdated @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
