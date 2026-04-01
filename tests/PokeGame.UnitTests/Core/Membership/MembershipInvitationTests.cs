using Bogus;
using Krakenar.Contracts.Users;
using Logitar;
using PokeGame.Builders;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

[Trait(Traits.Category, Categories.Unit)]
public class MembershipInvitationTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly User _invitee = new UserBuilder().Build();

  public MembershipInvitationTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "Accept: it should accept the invitation.")]
  public void Given_Pending_When_Accept_Then_Accepted()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithInvitee(_invitee).Build();
    invitation.Accept();
    Assert.Equal(MembershipInvitationStatus.Accepted, invitation.Status);
    Assert.Contains(invitation.Changes, change => change is MembershipInvitationAccepted accepted && accepted.ActorId == invitation.InviteeId?.ActorId);

    invitation.ClearChanges();
    invitation.Accept();
    Assert.False(invitation.HasChanges);
    Assert.Empty(invitation.Changes);
  }

  [Fact(DisplayName = "Accept: it should throw InvalidOperationException when the invitation has not invitee.")]
  public void Given_NoInvitee_When_Accept_Then_InvalidOperationException()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).Build();
    var exception = Assert.Throws<InvalidOperationException>(invitation.Accept);
    Assert.Equal("A membership invitation can only be accepted by the invitee.", exception.Message);
  }

  [Fact(DisplayName = "Accept: it should throw MembershipInvitationExpiredException when the invitation has expired.")]
  public void Given_Expired_When_Accept_Then_MembershipInvitationExpiredException()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithInvitee(_invitee).IsExpired().Build();
    var exception = Assert.Throws<MembershipInvitationExpiredException>(invitation.Accept);
    Assert.Equal(invitation.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(invitation.EntityId, exception.MembershipInvitationId);
    Assert.Equal(invitation.ExpiresOn?.AsUniversalTime(), exception.ExpiredOn);
  }

  [Fact(DisplayName = "Accept: it should throw MembershipInvitationNotPendingException when the invitation is not pending.")]
  public void Given_NotPending_When_Accept_Then_MembershipInvitationNotPendingException()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithInvitee(_invitee).IsCancelled().Build();
    var exception = Assert.Throws<MembershipInvitationNotPendingException>(invitation.Accept);
    Assert.Equal(invitation.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(invitation.EntityId, exception.MembershipInvitationId);
    Assert.Equal(invitation.Status, exception.Status);
  }

  [Fact(DisplayName = "Cancel: it should cancel the invitation.")]
  public void Given_Pending_When_Cancel_Then_Cancelled()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithWorld(_world).Build();
    invitation.Cancel(_world.OwnerId);
    Assert.Equal(MembershipInvitationStatus.Cancelled, invitation.Status);
    Assert.Contains(invitation.Changes, change => change is MembershipInvitationCancelled cancelled && cancelled.ActorId == _world.OwnerId.ActorId);

    invitation.ClearChanges();
    invitation.Cancel(_world.OwnerId);
    Assert.False(invitation.HasChanges);
    Assert.Empty(invitation.Changes);
  }

  [Fact(DisplayName = "Cancel: it should throw MembershipInvitationExpiredException when the invitation has expired.")]
  public void Given_Expired_When_Cancel_Then_MembershipInvitationExpiredException()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithWorld(_world).IsExpired().Build();
    var exception = Assert.Throws<MembershipInvitationExpiredException>(() => invitation.Cancel(_world.OwnerId));
    Assert.Equal(invitation.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(invitation.EntityId, exception.MembershipInvitationId);
    Assert.Equal(invitation.ExpiresOn?.AsUniversalTime(), exception.ExpiredOn);
  }

  [Fact(DisplayName = "Cancel: it should throw MembershipInvitationNotPendingException when the invitation is not pending.")]
  public void Given_NotPending_When_Cancel_Then_MembershipInvitationNotPendingException()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithWorld(_world).WithInvitee(_invitee).IsAccepted().Build();
    var exception = Assert.Throws<MembershipInvitationNotPendingException>(() => invitation.Cancel(_world.OwnerId));
    Assert.Equal(invitation.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(invitation.EntityId, exception.MembershipInvitationId);
    Assert.Equal(invitation.Status, exception.Status);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the expiration is not in the future.")]
  public void Given_ExpirationNotInTheFuture_When_ctor_Then_ArgumentOutOfRangeException()
  {
    ReadOnlyEmail email = new(_faker.Person.Email);
    DateTime expiresOn = DateTime.Now.AddDays(-1);
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new MembershipInvitation(_world, email, inviteeId: null, expiresOn));
    Assert.Equal("expiresOn", exception.ParamName);
  }

  [Theory(DisplayName = "IsExpired: it should return false when the invitation is not expired.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_NotExpired_When_IsExpired_Then_FalseReturned(bool hasExpiration)
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithExpiration(hasExpiration ? DateTime.Now.AddDays(1) : null).Build();
    Assert.False(invitation.IsExpired());
    Assert.False(invitation.IsExpired(DateTime.Now.AddMonths(-1)));
  }

  [Fact(DisplayName = "IsExpired: it should return true when the invitation is expired.")]
  public void Given_Expired_When_IsExpired_Then_TrueReturned()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).IsExpired().Build();
    Assert.True(invitation.IsExpired());
    Assert.True(invitation.IsExpired(DateTime.Now.AddMonths(1)));
  }
}
