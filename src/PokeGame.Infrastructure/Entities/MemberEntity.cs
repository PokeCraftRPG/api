using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MemberEntity
{
  public int MemberId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }

  public string MemberKey { get; private set; } = string.Empty;
  public Guid UserId { get; private set; }

  public string? GrantedBy { get; private set; }
  public DateTime GrantedOn { get; private set; }

  public string? RevokedBy { get; private set; }
  public DateTime? RevokedOn { get; private set; }

  public MemberEntity(WorldEntity world, WorldMembershipGranted @event)
  {
    World = world;
    WorldId = world.WorldId;

    MemberKey = @event.UserId.Value;
    UserId = @event.UserId.EntityId;

    Grant(@event);
  }

  private MemberEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(capacity: 3);
    actorIds.Add(new ActorId(MemberKey));
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

  public void Grant(WorldMembershipGranted @event)
  {
    GrantedBy = @event.ActorId?.Value;
    GrantedOn = @event.OccurredOn.AsUniversalTime();

    RevokedBy = null;
    RevokedOn = null;
  }

  public override bool Equals(object? obj) => obj is MemberEntity member && member.MemberId == MemberId;
  public override int GetHashCode() => MemberId.GetHashCode();
  public override string ToString() => $"{MemberKey} | {base.ToString()} (MemberId={MemberId})";
}
