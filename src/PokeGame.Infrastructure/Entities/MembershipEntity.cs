using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MembershipEntity
{
  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }

  public Guid UserId { get; private set; }
  public string MemberId { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public string? GrantedBy { get; private set; }
  public DateTime GrantedOn { get; private set; }

  public string? RevokedBy { get; private set; }
  public DateTime? RevokedOn { get; private set; }

  public MembershipEntity(WorldEntity world, UserId userId, DomainEvent @event)
  {
    World = world;
    WorldId = world.WorldId;

    UserId = userId.EntityId;
    MemberId = userId.Value;

    Grant(@event);
  }

  private MembershipEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(capacity: 3);
    actorIds.Add(new ActorId(MemberId));
    if (GrantedBy is not null)
    {
      actorIds.Add(new ActorId(GrantedBy));
    }
    if (RevokedBy is not null)
    {
      actorIds.Add(new ActorId(RevokedBy));
    }
    return actorIds;
  }

  public void Grant(DomainEvent @event)
  {
    IsActive = true;

    GrantedBy = @event.ActorId?.Value;
    GrantedOn = @event.OccurredOn.AsUniversalTime();
  }

  public void Revoke(WorldMembershipRevoked @event)
  {
    IsActive = false;

    RevokedBy = @event.ActorId?.Value;
    RevokedOn = @event.OccurredOn.AsUniversalTime();
  }

  public override bool Equals(object? obj) => obj is MembershipEntity membership && membership.WorldId == WorldId && membership.UserId == UserId;
  public override int GetHashCode() => HashCode.Combine(WorldId, UserId);
  public override string ToString() => $"{base.ToString()} (WorldId={WorldId}, UserId={UserId})";
}
