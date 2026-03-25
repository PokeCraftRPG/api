using Bogus;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class FriendshipTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Friendship.")]
  public void Given_ValidValue_When_ctor_Then_Friendship()
  {
    byte value = _faker.Random.Byte();
    Friendship friendship = new(value);
    Assert.Equal(value, friendship.Value);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Friendship_When_ToString_Then_CorrectValue()
  {
    byte value = _faker.Random.Byte();
    Friendship friendship = new(value);
    Assert.Equal(value.ToString(), friendship.ToString());
  }
}
