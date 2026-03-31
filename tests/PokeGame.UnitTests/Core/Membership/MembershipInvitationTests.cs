using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

[Trait(Traits.Category, Categories.Unit)]
public class MembershipInvitationTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public MembershipInvitationTests()
  {
    _world = new WorldBuilder(_faker).Build();
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
    ReadOnlyEmail email = new(_faker.Person.Email);
    DateTime? expiresOn = hasExpiration ? DateTime.Now.AddDays(1) : null;
    MembershipInvitation invitation = new(_world, email, inviteeId: null, expiresOn);
    Assert.False(invitation.IsExpired());
    Assert.False(invitation.IsExpired(DateTime.Now.AddMonths(-1)));
  }

  [Fact(DisplayName = "IsExpired: it should return true when the invitation is expired.")]
  public void Given_Expired_When_IsExpired_Then_TrueReturned()
  {
    const int LifetimeMilliseconds = 50;

    ReadOnlyEmail email = new(_faker.Person.Email);
    DateTime expiresOn = DateTime.Now.AddMilliseconds(LifetimeMilliseconds);
    MembershipInvitation invitation = new(_world, email, inviteeId: null, expiresOn);

    Thread.Sleep(LifetimeMilliseconds);

    Assert.True(invitation.IsExpired());
    Assert.True(invitation.IsExpired(DateTime.Now.AddMonths(1)));
  }
}
