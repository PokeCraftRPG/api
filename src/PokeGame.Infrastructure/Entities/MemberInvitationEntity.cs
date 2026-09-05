using Logitar;
using PokeGame.Core.Membership;
using PokeGame.Core.Membership.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MemberInvitationEntity : AggregateEntity
{
  public int MemberInvitationId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string? EmailAddress { get; private set; }
  public string? UserId { get; private set; }

  public MemberInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MemberInvitationEntity(int worldId, MemberInvitationSent @event) : base(@event)
  {
    WorldId = worldId;
    Id = new MemberInvitationId(@event.StreamId).EntityId;

    EmailAddress = @event.EmailAddress?.Value;
    UserId = @event.UserId?.Value;

    Status = MemberInvitationStatus.Pending;
    ExpiresOn = @event.ExpiresOn?.AsUniversalTime();
  }

  private MemberInvitationEntity() : base()
  {
  }

  public void Accept(MemberInvitationAccepted @event)
  {
    Update(@event);

    Status = MemberInvitationStatus.Accepted;
  }

  public void Cancel(MemberInvitationCancelled @event)
  {
    Update(@event);

    Status = MemberInvitationStatus.Cancelled;
  }

  public void Decline(MemberInvitationDeclined @event)
  {
    Update(@event);

    Status = MemberInvitationStatus.Declined;
  }
}
