using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  private WorldUpdated _updated = new();
  private bool HasUpdates => _updated.Name is not null || _updated.Description is not null;

  public new WorldId Id => new(base.Id);

  public UserId OwnerId { get; private set; }

  private Name? _name = null;
  public Name Name
  {
    get => _name ?? throw new InvalidOperationException("The world was not initialized.");
    set
    {
      if (_name != value)
      {
        _name = value;
        _updated.Name = value;
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

  public long Size => Name.Size + (Description?.Size ?? 0);

  public World() : base()
  {
  }

  public World(UserId ownerId, Name name, WorldId? worldId = null)
    : base((worldId ?? WorldId.NewId()).StreamId)
  {
    Raise(new WorldCreated(ownerId, name), ownerId.ActorId);
  }
  protected virtual void Handle(WorldCreated @event)
  {
    OwnerId = @event.OwnerId;

    _name = @event.Name;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new WorldDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, Id.ToGuid(), worldId: null, Size);

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new();
    }
  }
  protected virtual void Handle(WorldUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }
  }

  public override string ToString() => $"{Name} | {base.ToString()}";
}
