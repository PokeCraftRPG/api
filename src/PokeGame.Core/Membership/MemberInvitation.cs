using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class MemberInvitation : AggregateRoot
{
  public const string EntityKind = "MemberInvitation";

  public new MemberInvitationId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public EmailAddress? EmailAddress { get; private set; }
  public UserId? UserId { get; private set; }

  public MemberInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MemberInvitation() : base()
  {
  }

  public MemberInvitation(MemberInvitationId membershipInvitationId, EmailAddress emailAddress, DateTime? expiresOn = null, ActorId? actorId = null)
    : base(membershipInvitationId.StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn), "The expiration must be a date and time set in the future.");
    }

    Raise(new MemberInvitationSent(emailAddress, UserId: null, expiresOn), actorId);
  }
  public MemberInvitation(MemberInvitationId membershipInvitationId, UserId userId, DateTime? expiresOn = null, ActorId? actorId = null)
    : base(membershipInvitationId.StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn), "The expiration must be a date and time set in the future.");
    }

    Raise(new MemberInvitationSent(EmailAddress: null, userId, expiresOn), actorId);
  }
  private void Handle(MemberInvitationSent @event)
  {
    EmailAddress = @event.EmailAddress;
    UserId = @event.UserId;

    Status = MemberInvitationStatus.Pending;
    ExpiresOn = @event.ExpiresOn;
  }

  public void Accept(ActorId? actorId = null)
  {
    if (Status != MemberInvitationStatus.Pending || Status != MemberInvitationStatus.Accepted)
    {
      throw new InvalidMemberInvitationStatusException(this);
    }
    else if (IsExpired())
    {
      throw new MemberInvitationExpiredException(this);
    }
    else if (Status == MemberInvitationStatus.Pending)
    {
      Raise(new MemberInvitationAccepted(), actorId);
    }
  }
  private void Handle(MemberInvitationAccepted _)
  {
    Status = MemberInvitationStatus.Accepted;
  }

  public void Cancel(ActorId? actorId = null)
  {
    if (Status != MemberInvitationStatus.Pending || Status != MemberInvitationStatus.Cancelled)
    {
      throw new InvalidMemberInvitationStatusException(this);
    }
    else if (IsExpired())
    {
      throw new MemberInvitationExpiredException(this);
    }
    else if (Status == MemberInvitationStatus.Pending)
    {
      Raise(new MemberInvitationCancelled(), actorId);
    }
  }
  private void Handle(MemberInvitationCancelled _)
  {
    Status = MemberInvitationStatus.Cancelled;
  }

  public void Decline(ActorId? actorId = null)
  {
    if (Status != MemberInvitationStatus.Pending || Status != MemberInvitationStatus.Declined)
    {
      throw new InvalidMemberInvitationStatusException(this);
    }
    else if (IsExpired())
    {
      throw new MemberInvitationExpiredException(this);
    }
    else if (Status == MemberInvitationStatus.Pending)
    {
      Raise(new MemberInvitationDeclined(), actorId);
    }
  }
  private void Handle(MemberInvitationDeclined _)
  {
    Status = MemberInvitationStatus.Declined;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new MemberInvitationDeleted(), actorId);
    }
  }

  public bool IsExpired(DateTime? moment = null) => ExpiresOn.HasValue && ExpiresOn.Value.AsUniversalTime() <= (moment?.AsUniversalTime() ?? DateTime.UtcNow);
}
