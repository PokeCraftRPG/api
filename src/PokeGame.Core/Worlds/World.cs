using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  public new WorldId Id => new(base.Id);
  public Guid EntityId => Id.EntityId;

  public UserId OwnerId { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The key was not initialized.");
  public Name? Name { get; private set; }
  public Description? Description { get; private set; }

  public World() : base()
  {
  }

  public World(UserId ownerId, Slug key, WorldId? worldId = null)
    : base((worldId ?? WorldId.NewId()).StreamId)
  {
    Raise(new WorldCreated(ownerId, key), ownerId.ActorId);
  }
  protected virtual void Handle(WorldCreated @event)
  {
    OwnerId = @event.OwnerId;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new WorldDeleted(), actorId);
    }
  }

  public void Describe(Description? description, ActorId? actorId = null)
  {
    if (!Equals(Description, description))
    {
      Raise(new WorldDescribed(description), actorId);
    }
  }
  protected virtual void Handle(WorldDescribed @event)
  {
    Description = @event.Description;
  }

  public Entity GetEntity() => new(EntityKind, EntityId);

  public void Rename(Name? name, ActorId? actorId = null)
  {
    if (!Equals(Name, name))
    {
      Raise(new WorldRenamed(name), actorId);
    }
  }
  protected virtual void Handle(WorldRenamed @event)
  {
    Name = @event.Name;
  }

  public void SetKey(Slug key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new WorldKeyChanged(key), actorId);
    }
  }
  protected virtual void Handle(WorldKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
