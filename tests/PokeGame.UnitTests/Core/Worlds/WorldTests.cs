using Bogus;
using Krakenar.Contracts.Users;
using PokeGame.Builders;
using PokeGame.Core.Actors;

namespace PokeGame.Core.Worlds;

[Trait(Traits.Category, Categories.Unit)]
public class WorldTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly User _user;

  public WorldTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _user = new UserBuilder().Build();
  }

  [Fact(DisplayName = "IsMember: it should return false when the user is not a member.")]
  public void Given_NotMember_When_IsMember_Then_FalseReturned()
  {
    Assert.False(_world.IsMember(_user.GetUserId()));
  }

  [Fact(DisplayName = "IsMember: it should return true when the user is a member.")]
  public void GivenMember_When_IsMember_Then_TrueReturned()
  {
    _world.GrantMembership(_user.GetUserId(), _world.OwnerId);
    Assert.True(_world.IsMember(_user.GetUserId()));
  }

  [Fact(DisplayName = "RevokeMembership: it should throw ArgumentException when the user is not member.")]
  public void Given_NotMember_When_RevokeMembership_Then_ArgumentException()
  {
    UserId memberId = _user.GetUserId();
    var exception = Assert.Throws<ArgumentException>(() => _world.RevokeMembership(memberId, _world.OwnerId));
    Assert.Equal("memberId", exception.ParamName);
    Assert.StartsWith($"The user 'Id={memberId}' is not a member.", exception.Message);
  }

  [Fact(DisplayName = "TransferOwnership: it should throw ArgumentException when the user is not member.")]
  public void Given_NotMember_When_TransferOwnership_Then_ArgumentException()
  {
    UserId memberId = _user.GetUserId();
    var exception = Assert.Throws<ArgumentException>(() => _world.TransferOwnership(memberId, _world.OwnerId));
    Assert.Equal("memberId", exception.ParamName);
    Assert.StartsWith($"The user 'Id={memberId}' is not a member.", exception.Message);
  }
}
