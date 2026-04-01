using Bogus;
using Krakenar.Contracts.Users;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IMembershipInvitationBuilder
{
  IMembershipInvitationBuilder WithId(MembershipInvitationId? id);
  IMembershipInvitationBuilder WithWorld(World? world);
  IMembershipInvitationBuilder WithEmail(IEmail? email);
  IMembershipInvitationBuilder WithInvitee(User? user);
  IMembershipInvitationBuilder WithInvitee(UserId? inviteeId);
  IMembershipInvitationBuilder WithStatus(MembershipInvitationStatus status);
  IMembershipInvitationBuilder IsAccepted(bool isAccepted = true);
  IMembershipInvitationBuilder IsCancelled(bool isCancelled = true);
  IMembershipInvitationBuilder IsPending(bool isPending = true);
  IMembershipInvitationBuilder WithExpiration(DateTime? expiresOn);
  IMembershipInvitationBuilder IsExpired(bool isExpired = true);

  MembershipInvitation Build();
}

public class MembershipInvitationBuilder : IMembershipInvitationBuilder
{
  private const int MillisecondsDelay = 50;

  private readonly Faker _faker;

  private IEmail? _email = null;
  private DateTime? _expiresOn = null;
  private MembershipInvitationId? _id = null;
  private UserId? _inviteeId = null;
  private bool _isExpired = false;
  private MembershipInvitationStatus _status = MembershipInvitationStatus.Pending;
  private World? _world = null;

  public MembershipInvitationBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IMembershipInvitationBuilder WithId(MembershipInvitationId? id)
  {
    _id = id;
    return this;
  }

  public IMembershipInvitationBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IMembershipInvitationBuilder WithEmail(IEmail? email)
  {
    _email = email;
    return this;
  }

  public IMembershipInvitationBuilder WithInvitee(User? user)
  {
    _inviteeId = user?.GetUserId();
    return this;
  }
  public IMembershipInvitationBuilder WithInvitee(UserId? inviteeId)
  {
    _inviteeId = inviteeId;
    return this;
  }

  public IMembershipInvitationBuilder WithStatus(MembershipInvitationStatus status)
  {
    _status = status;
    return this;
  }
  public IMembershipInvitationBuilder IsAccepted(bool isAccepted = true)
  {
    _status = isAccepted ? MembershipInvitationStatus.Accepted : MembershipInvitationStatus.Pending;
    return this;
  }
  public IMembershipInvitationBuilder IsCancelled(bool isCancelled = true)
  {
    _status = isCancelled ? MembershipInvitationStatus.Cancelled : MembershipInvitationStatus.Pending;
    return this;
  }
  public IMembershipInvitationBuilder IsPending(bool isPending = true)
  {
    _status = isPending ? MembershipInvitationStatus.Pending : MembershipInvitationStatus.Cancelled;
    return this;
  }

  public IMembershipInvitationBuilder WithExpiration(DateTime? expiresOn)
  {
    _expiresOn = expiresOn;
    return this;
  }

  public IMembershipInvitationBuilder IsExpired(bool isExpired = true)
  {
    _isExpired = true;
    return this;
  }

  public MembershipInvitation Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ReadOnlyEmail email = _email is null ? new(_faker.Internet.Email()) : new(_email.Address, _email.IsVerified);
    DateTime? expiresOn = _expiresOn ?? (_isExpired ? DateTime.Now.AddMilliseconds(MillisecondsDelay) : null);

    MembershipInvitation invitation = _id.HasValue ? new(email, _inviteeId, expiresOn, world.OwnerId, _id.Value) : new(world, email, _inviteeId, expiresOn);

    if (_isExpired)
    {
      Thread.Sleep(MillisecondsDelay);
    }

    switch (_status)
    {
      case MembershipInvitationStatus.Accepted:
        invitation.Accept();
        break;
      case MembershipInvitationStatus.Cancelled:
        invitation.Cancel(world.OwnerId);
        break;
      case MembershipInvitationStatus.Pending:
        break;
      default:
        throw new NotSupportedException($"The membership invitation status '{_status}' is not supported.");
    }

    return invitation;
  }
}
