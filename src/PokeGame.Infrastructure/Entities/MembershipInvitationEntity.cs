using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MembershipInvitationEntity : AggregateEntity
{
  public int MembershipInvitationId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string EmailAddress { get; private set; } = string.Empty;
  public string EmailAddressNormalized
  {
    get => EmailAddress.Trim().ToLowerInvariant();
    private set { }
  }
  public string? InviteeId { get; private set; }
  public Guid? UserId { get; private set; }

  public MembershipInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MembershipInvitationEntity(WorldEntity world, MembershipInvitationCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    Id = new MembershipInvitationId(@event.StreamId).EntityId;

    EmailAddress = @event.Email.Address;
    InviteeId = @event.InviteeId?.Value;
    UserId = @event.InviteeId?.EntityId;

    Status = MembershipInvitationStatus.Pending;
    ExpiresOn = @event.ExpiresOn?.AsUniversalTime();
  }

  private MembershipInvitationEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (InviteeId is not null)
    {
      actorIds.Add(new ActorId(InviteeId));
    }
    return actorIds;
  }

  public void Accept(MembershipInvitationAccepted @event)
  {
    base.Update(@event);

    Status = MembershipInvitationStatus.Accepted;
  }

  public void Cancel(MembershipInvitationCancelled @event)
  {
    base.Update(@event);

    Status = MembershipInvitationStatus.Cancelled;
  }

  public void Decline(MembershipInvitationDeclined @event)
  {
    base.Update(@event);

    Status = MembershipInvitationStatus.Declined;
  }

  public override string ToString() => $"{EmailAddress} | {base.ToString()}";
}
