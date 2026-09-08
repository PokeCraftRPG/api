using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Core.Worlds;

public sealed class World : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "World";

  public new WorldId Id => new(base.Id);
  public Guid EntityId => Id.EntityId;

  public UserId OwnerId { get; private set; }
  private readonly HashSet<UserId> _memberIds = [];
  public IReadOnlySet<UserId> MemberIds => _memberIds.AsReadOnly();

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
    _memberIds.Add(@event.OwnerId);

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

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new WorldDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(WorldDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

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

  #region Membership
  public void GrantMembership(UserId userId, ActorId? actorId = null)
  {
    if (!IsMember(userId))
    {
      Raise(new WorldMembershipGranted(userId), actorId);
    }
  }
  private void Handle(WorldMembershipGranted @event)
  {
    _memberIds.Add(@event.UserId);
  }

  public bool IsMember(UserId userId) => _memberIds.Contains(userId);

  public void LeaveMembership(UserId userId, ActorId? actorId = null)
  {
    if (userId == OwnerId)
    {
      throw new OwnerCannotLeaveWorldException(this);
    }
    else if (IsMember(userId))
    {
      Raise(new WorldMembershipLeft(userId), actorId);
    }
  }
  private void Handle(WorldMembershipLeft @event)
  {
    _memberIds.Remove(@event.UserId);
  }

  public void RevokeMembership(UserId userId, ActorId? actorId = null)
  {
    if (userId == OwnerId)
    {
      throw new WorldOwnershipCannotBeRevokedException(this);
    }
    else if (IsMember(userId))
    {
      Raise(new WorldMembershipRevoked(userId), actorId);
    }
  }
  private void Handle(WorldMembershipRevoked @event)
  {
    _memberIds.Remove(@event.UserId);
  }

  public void TransferOwnership(UserId userId, ActorId? actorId = null)
  {
    if (OwnerId != userId)
    {
      if (!IsMember(userId))
      {
        throw new UserIsNotMemberException(this, userId);
      }

      Raise(new WorldOwnershipTransferred(userId), actorId);
    }
  }
  private void Handle(WorldOwnershipTransferred @event)
  {
    OwnerId = @event.UserId;
  }
  #endregion

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
