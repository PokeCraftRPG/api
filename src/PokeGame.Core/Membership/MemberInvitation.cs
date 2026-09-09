using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public sealed class MemberInvitation : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "MemberInvitation";

  public new MemberInvitationId Id => new(base.Id);
  public Guid EntityId => Id.EntityId;

  public WorldId WorldId { get; private set; }

  private EmailAddress? _emailAddress = null;
  public EmailAddress EmailAddress => _emailAddress ?? throw new InvalidOperationException("The email address was not initialized.");
  public UserId? UserId { get; private set; }

  public MemberInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MemberInvitation() : base()
  {
  }

  public MemberInvitation(
    World world,
    EmailAddress emailAddress,
    UserId? userId = null,
    DateTime? expiresOn = null,
    ActorId? actorId = null,
    MemberInvitationId? memberInvitationId = null) : base((memberInvitationId ?? MemberInvitationId.NewId()).StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn), "The expiration must be a date and time set in the future.");
    }

    Raise(new MemberInvitationSent(world.Id, emailAddress, userId, expiresOn), actorId);
  }
  private void Handle(MemberInvitationSent @event)
  {
    WorldId = @event.WorldId;

    _emailAddress = @event.EmailAddress;
    UserId = @event.UserId;

    Status = MemberInvitationStatus.Pending;
    ExpiresOn = @event.ExpiresOn;
  }

  public void Accept(ActorId? actorId = null)
  {
    if (Status != MemberInvitationStatus.Pending && Status != MemberInvitationStatus.Accepted)
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
    if (Status != MemberInvitationStatus.Pending && Status != MemberInvitationStatus.Cancelled)
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

  public void Claim(UserId userId)
  {
    if (UserId != userId)
    {
      if (UserId.HasValue)
      {
        throw new InvalidOperationException($"The member invitation 'Id={Id}' has already been claimed by user 'Id={UserId}'.");
      }

      Raise(new MemberInvitationClaimed(userId), userId.ActorId);
    }
  }
  private void Handle(MemberInvitationClaimed @event)
  {
    UserId = @event.UserId;
  }

  public void Decline(ActorId? actorId = null)
  {
    if (Status != MemberInvitationStatus.Pending && Status != MemberInvitationStatus.Declined)
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

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public bool IsExpired(DateTime? moment = null) => ExpiresOn.HasValue && ExpiresOn.Value.AsUniversalTime() <= (moment?.AsUniversalTime() ?? DateTime.UtcNow);
}
