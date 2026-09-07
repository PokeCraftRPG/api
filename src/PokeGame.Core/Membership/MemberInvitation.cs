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

  public EmailAddress? EmailAddress { get; private set; }
  public UserId? UserId { get; private set; }

  public MemberInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MemberInvitation() : base()
  {
  }

  public MemberInvitation(World world, EmailAddress emailAddress, DateTime? expiresOn = null, ActorId? actorId = null, MemberInvitationId? memberInvitationId = null)
    : base((memberInvitationId ?? MemberInvitationId.NewId()).StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn), "The expiration must be a date and time set in the future.");
    }

    Raise(new MemberInvitationSent(world.Id, emailAddress, UserId: null, expiresOn), actorId);
  }
  public MemberInvitation(World world, UserId userId, DateTime? expiresOn = null, ActorId? actorId = null, MemberInvitationId? memberInvitationId = null)
    : base((memberInvitationId ?? MemberInvitationId.NewId()).StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn), "The expiration must be a date and time set in the future.");
    }

    Raise(new MemberInvitationSent(world.Id, EmailAddress: null, userId, expiresOn), actorId);
  }
  private void Handle(MemberInvitationSent @event)
  {
    WorldId = @event.WorldId;

    EmailAddress = @event.EmailAddress;
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
