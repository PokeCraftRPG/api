using Bogus;
using Krakenar.Contracts.Actors;
using Krakenar.Contracts.ApiKeys;
using Krakenar.Contracts.Users;
using Logitar.EventSourcing;
using PokeGame.Builders;
using PokeGame.Core.Actors;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class UserIdTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should construct a User ID from a User actor.")]
  public void Given_User_When_ctor_Then_UserId()
  {
    User user = new UserBuilder(_faker).Build();
    Actor actor = new(user);
    ActorId actorId = actor.GetActorId();
    UserId userId = user.GetUserId();
    Assert.Equal(actorId.Value, userId.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentException when the actor is an API Key.")]
  public void Given_ApiKey_When_ctor_Then_ArgumentException()
  {
    ApiKey apiKey = new ApiKeyBuilder(_faker).Build();
    Actor actor = new(apiKey);
    ActorId actorId = actor.GetActorId();
    var exception = Assert.Throws<ArgumentException>(() => new UserId(actorId));
    Assert.Equal("actorId", exception.ParamName);
    Assert.StartsWith("The actor must be a user.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentException when the actor is the System.")]
  public void Given_System_When_ctor_Then_ArgumentException()
  {
    Actor actor = new();
    ActorId actorId = actor.GetActorId();
    var exception = Assert.Throws<ArgumentException>(() => new UserId(actorId));
    Assert.Equal("actorId", exception.ParamName);
    Assert.StartsWith("The actor must be a user.", exception.Message);
  }
}
