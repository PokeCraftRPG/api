using Bogus;
using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Actors;

[Trait(Traits.Category, Categories.Unit)]
public class ActorExtensionsTests
{
  private readonly Faker _faker = new();

  [Theory(DisplayName = "GetActorId: it should return the correct identifier.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_Actor_When_GetActorId_Then_CorrectId(bool hasRealmId)
  {
    Actor actor = new()
    {
      RealmId = hasRealmId ? Guid.NewGuid() : null,
      Type = _faker.Random.Enum<ActorType>(),
      Id = Guid.NewGuid()
    };

    ActorId actorId = actor.GetActorId();

    string[] values = actorId.Value.Split('|');
    if (actor.RealmId.HasValue)
    {
      Assert.Equal(2, values.Length);
      Assert.Equal(new Entity("Realm", actor.RealmId.Value).ToString(), values.First());
    }
    Assert.Equal(new Entity(actor.Type.ToString(), actor.Id).ToString(), values.Last());
  }

  [Theory(DisplayName = "ToActor: it should return a new actor.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_ActorId_When_ToActor_Then_Actor(bool hasRealmId)
  {
    Guid? realmId = hasRealmId ? Guid.NewGuid() : null;
    ActorType type = _faker.Random.Enum<ActorType>();
    Guid id = Guid.NewGuid();
    string entity = new Entity(type.ToString(), id).ToString();
    ActorId actorId = new(realmId.HasValue ? string.Join('|', new Entity("Realm", realmId.Value), entity) : entity);

    Actor actor = actorId.ToActor();

    Assert.Equal(realmId, actor.RealmId);
    Assert.Equal(type, actor.Type);
    Assert.Equal(id, actor.Id);
  }

  [Fact(DisplayName = "ToActor: it should throw ArgumentException when the type is not valid.")]
  public void Given_InvalidType_When_ToActor_Then_ArgumentException()
  {
    string actorId = WorldId.NewId().Value;
    var exception = Assert.Throws<ArgumentException>(() => new ActorId(actorId).ToActor());
    Assert.Equal("actorId", exception.ParamName);
    Assert.StartsWith("The actor type 'World' is not valid.", exception.Message);
  }

  [Fact(DisplayName = "ToActor: it should throw ArgumentException when the value is not valid.")]
  public void Given_InvalidValue_When_ToActor_Then_ArgumentException()
  {
    string actorId = "a|b|c";
    var exception = Assert.Throws<ArgumentException>(() => new ActorId(actorId).ToActor());
    Assert.Equal("actorId", exception.ParamName);
    Assert.StartsWith($"The value '{actorId}' is not a valid actor identifier.", exception.Message);
  }
}
