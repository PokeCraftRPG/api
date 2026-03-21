using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  private WorldUpdated _updated = new();
  private bool HasUpdates => _updated.Slug is not null || _updated.Name is not null || _updated.Description is not null;

  public new WorldId Id => new(base.Id);

  public UserId OwnerId { get; private set; }

  private Slug? _slug = null;
  public Slug Slug
  {
    get => _slug ?? throw new InvalidOperationException("The world was not initialized.");
    set
    {
      if (_slug != value)
      {
        _slug = value;
        _updated.Slug = value;
      }
    }
  }
  private Name? _name = null;
  public Name? Name
  {
    get => _name;
    set
    {
      if (_name != value)
      {
        _name = value;
        _updated.Name = new Optional<Name>(value);
      }
    }
  }
  private Description? _description = null;
  public Description? Description
  {
    get => _description;
    set
    {
      if (_description != value)
      {
        _description = value;
        _updated.Description = new Optional<Description>(value);
      }
    }
  }

  public World() : base()
  {
  }

  public World(UserId ownerId, Slug slug, WorldId? worldId = null)
    : base((worldId ?? WorldId.NewId()).StreamId)
  {
    Raise(new WorldCreated(ownerId, slug), ownerId.ActorId);
  }
  protected virtual void Handle(WorldCreated @event)
  {
    OwnerId = @event.OwnerId;

    _slug = @event.Slug;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new WorldDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, Id.ToGuid());

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId);
      _updated = new();
    }
  }
  protected virtual void Handle(WorldUpdated @event)
  {
    if (@event.Slug is not null)
    {
      _slug = @event.Slug;
    }
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Slug.Value} | {base.ToString()}";
}
