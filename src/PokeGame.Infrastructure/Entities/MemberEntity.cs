using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MemberEntity
{
  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }

  public string UserId { get; private set; } = string.Empty;

  public string? GrantedBy { get; private set; }
  public DateTime GrantedOn { get; private set; }

  public MemberEntity(WorldEntity world, WorldMembershipGranted @event) : this(world, (DomainEvent)@event)
  {
    UserId = @event.UserId.Value;
  }

  public MemberEntity(WorldEntity world, WorldOwnershipTransferred @event) : this(world, (DomainEvent)@event)
  {
    UserId = world.OwnerId;
  }

  private MemberEntity(WorldEntity world, DomainEvent @event)
  {
    World = world;
    WorldId = world.WorldId;

    GrantedBy = @event.ActorId?.Value;
    GrantedOn = @event.OccurredOn.AsUniversalTime();
  }

  private MemberEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(capacity: 2);
    actorIds.Add(new ActorId(UserId));
    if (GrantedBy is not null)
    {
      actorIds.Add(new ActorId(GrantedBy));
    }
    return actorIds;
  }

  public override bool Equals(object? obj) => obj is MemberEntity member && member.WorldId == WorldId && member.UserId == UserId;
  public override int GetHashCode() => HashCode.Combine(WorldId, UserId);
  public override string ToString() => $"{base.ToString()} (WorldId={WorldId}, UserId={UserId})";
}
