using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public sealed class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  public new WorldId Id => new(base.Id);
  public Guid EntityId => Id.EntityId;

  public UserId OwnerId { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public World(UserId ownerId, Key key, WorldId? worldId = null)
    : base((worldId ?? WorldId.NewId()).StreamId)
  {
    Raise(new WorldCreated(ownerId, key), ownerId.ActorId);
  }
  private void Handle(WorldCreated @event)
  {
    OwnerId = @event.OwnerId;

    _key = @event.Key;
  }

  public World() : base()
  {
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new WorldDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId);

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new WorldKeyChanged(key), actorId);
    }
  }
  private void Handle(WorldKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new WorldUpdated(name, summary, content), actorId);
    }
  }
  private void Handle(WorldUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
