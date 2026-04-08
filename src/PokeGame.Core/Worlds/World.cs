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

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The world was not initialized.");
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

  private readonly HashSet<UserId> _members = [];
  public IReadOnlyCollection<UserId> Members => _members.ToList().AsReadOnly();

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0);

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

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new WorldDeleted(), userId.ActorId);
    }
  }

  public UserId? FindMember(Guid userId)
  {
    UserId[] userIds = _members.Where(x => x.EntityId == userId).ToArray();
    return userIds.Length == 1 ? userIds.Single() : null;
  }

  public Entity GetEntity() => new(EntityKind, Id.ToGuid(), worldId: null, Size);

  public void GrantMembership(UserId memberId, UserId userId)
  {
    if (!IsMember(memberId))
    {
      Raise(new WorldMembershipGranted(memberId), userId.ActorId);
    }
  }
  protected virtual void Handle(WorldMembershipGranted @event)
  {
    _members.Add(@event.UserId);
  }

  public bool IsMember(UserId userId) => _members.Contains(userId);

  public void RevokeMembership(UserId memberId, UserId userId)
  {
    if (!IsMember(memberId))
    {
      throw new ArgumentException($"The user 'Id={memberId}' is not a member.", nameof(memberId));
    }
    Raise(new WorldMembershipRevoked(memberId), userId.ActorId);
  }
  protected virtual void Handle(WorldMembershipRevoked @event)
  {
    _members.Remove(@event.UserId);
  }

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new WorldKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(WorldKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void TransferOwnership(UserId memberId, UserId userId)
  {
    if (!IsMember(memberId))
    {
      throw new ArgumentException($"The user 'Id={memberId}' is not a member.", nameof(memberId));
    }
    else if (OwnerId != memberId)
    {
      Raise(new WorldOwnershipTransferred(memberId), userId.ActorId);
    }
  }
  protected virtual void Handle(WorldOwnershipTransferred @event)
  {
    _members.Add(OwnerId);
    _members.Remove(@event.OwnerId);
    OwnerId = @event.OwnerId;
  }

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
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
