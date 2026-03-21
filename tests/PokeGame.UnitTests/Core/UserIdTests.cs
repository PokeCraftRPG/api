using Logitar.EventSourcing;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class UserIdTests
{
  [Fact(DisplayName = "UserId constructor (string): it should expose the same value on Value and ToString.")]
  public void Given_string_When_ctor_Then_ValueAndToStringMatch()
  {
    UserId userId = new UserId("actor-123");

    Assert.Equal("actor-123", userId.Value);
    Assert.Equal("actor-123", userId.ToString());
  }

  [Fact(DisplayName = "UserId constructor (ActorId): it should preserve the ActorId instance.")]
  public void Given_actorId_When_ctor_Then_ActorIdMatches()
  {
    ActorId actorId = new ActorId("actor-456");
    UserId userId = new UserId(actorId);

    Assert.Equal(actorId, userId.ActorId);
    Assert.Equal("actor-456", userId.Value);
  }

  [Fact(DisplayName = "UserId equality: it should treat equal values as equal via == and object.Equals.")]
  public void Given_sameValue_When_comparing_Then_areEqual()
  {
    UserId left = new UserId("same");
    UserId right = new UserId("same");

    Assert.True(left == right);
    Assert.False(left != right);
    Assert.True(left.Equals(right));
    Assert.Equal(left.GetHashCode(), right.GetHashCode());
  }

  [Fact(DisplayName = "UserId equality: it should treat different values as not equal.")]
  public void Given_differentValues_When_comparing_Then_areNotEqual()
  {
    UserId left = new UserId("a");
    UserId right = new UserId("b");

    Assert.False(left == right);
    Assert.True(left != right);
    Assert.False(left.Equals(right));
  }

  [Fact(DisplayName = "UserId.Equals: it should return false for null and non-UserId objects.")]
  public void Given_nullOrOtherType_When_Equals_Then_returnsFalse()
  {
    UserId userId = new UserId("x");

    Assert.False(userId.Equals(null));
    Assert.False(userId.Equals("x"));
  }

  [Fact(DisplayName = "UserId default: it should equal another default UserId.")]
  public void Given_defaultUserId_When_comparingToDefault_Then_areEqual()
  {
    UserId a = default;
    UserId b = default;

    Assert.True(a == b);
    Assert.True(a.Equals(b));
  }
}
