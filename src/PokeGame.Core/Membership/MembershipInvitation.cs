using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public class MembershipInvitation : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "MembershipInvitation";

  public new MembershipInvitationId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private ReadOnlyEmail? _email = null;
  public ReadOnlyEmail Email => _email ?? throw new InvalidOperationException("The membership invitation was not initialized.");
  public UserId? InviteeId { get; private set; }

  public MembershipInvitationStatus Status { get; private set; }
  public DateTime? ExpiresOn { get; private set; }

  public MembershipInvitation() : base()
  {
  }

  public MembershipInvitation(World world, ReadOnlyEmail email, UserId? inviteeId, DateTime? expiresOn)
    : this(email, inviteeId, expiresOn, world.OwnerId, MembershipInvitationId.NewId(world.Id))
  {
  }

  public MembershipInvitation(ReadOnlyEmail email, UserId? inviteeId, DateTime? expiresOn, UserId userId, MembershipInvitationId membershipInvitationId)
    : base(membershipInvitationId.StreamId)
  {
    if (expiresOn.HasValue && expiresOn.Value.AsUniversalTime() <= DateTime.UtcNow)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresOn));
    }

    Raise(new MembershipInvitationCreated(email, inviteeId, expiresOn), userId.ActorId);
  }
  protected virtual void Handle(MembershipInvitationCreated @event)
  {
    _email = @event.Email;
    InviteeId = @event.InviteeId;

    Status = MembershipInvitationStatus.Pending;
    ExpiresOn = @event.ExpiresOn;
  }

  public void Accept()
  {
    if (!InviteeId.HasValue)
    {
      throw new InvalidOperationException("A membership invitation can only be accepted by the invitee.");
    }
    else if (IsExpired())
    {
      throw new MembershipInvitationExpiredException(this);
    }

    switch (Status)
    {
      case MembershipInvitationStatus.Accepted:
        return;
      case MembershipInvitationStatus.Pending:
        Raise(new MembershipInvitationAccepted(), InviteeId.Value.ActorId);
        return;
      default:
        throw new MembershipInvitationNotPendingException(this);
    }
  }
  protected virtual void Handle(MembershipInvitationAccepted _)
  {
    Status = MembershipInvitationStatus.Accepted;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new MembershipInvitationDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public bool IsExpired(DateTime? moment = null)
  {
    if (!ExpiresOn.HasValue)
    {
      return false;
    }

    moment = (moment ?? DateTime.Now).AsUniversalTime();
    return moment.Value >= ExpiresOn.Value.AsUniversalTime();
  }

  public override string ToString() => $"{Email} | {base.ToString()}";
}
