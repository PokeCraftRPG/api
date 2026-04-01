using Bogus;
using Krakenar.Contracts.Users;
using PokeGame.Builders;
using PokeGame.Core.Actors;

namespace PokeGame.Core.Worlds;

[Trait(Traits.Category, Categories.Unit)]
public class WorldTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "IsMember: it should return false when the user is not a member.")]
  public void Given_NotMember_When_IsMember_Then_FalseReturned()
  {
    World world = new WorldBuilder(_faker).Build();
    User user = new UserBuilder().Build();
    Assert.False(world.IsMember(user.GetUserId()));
  }

  [Fact(DisplayName = "IsMember: it should return true when the user is a member.")]
  public void GivenMember_When_IsMember_Then_TrueReturned()
  {
    World world = new WorldBuilder(_faker).Build();
    User user = new UserBuilder().Build();
    world.GrantMembership(user.GetUserId(), world.OwnerId);
    Assert.True(world.IsMember(user.GetUserId()));
  }
}
