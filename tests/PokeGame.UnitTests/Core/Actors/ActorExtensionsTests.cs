using Krakenar.Contracts.Actors;
using Logitar.EventSourcing;

namespace PokeGame.Core.Actors;

[Trait(Traits.Category, Categories.Unit)]
public class ActorExtensionsTests
{
  [Fact(DisplayName = "GetActorId: it should produce an ActorId that round-trips without a realm.")]
  public void Given_actorWithoutRealm_When_GetActorIdThenToActor_Then_matchesOriginal()
  {
    Guid id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    Actor actor = new Actor { Id = id, Type = ActorType.User, RealmId = null };

    ActorId actorId = actor.GetActorId();
    Actor roundTrip = actorId.ToActor();

    Assert.Null(roundTrip.RealmId);
    Assert.Equal(id, roundTrip.Id);
    Assert.Equal(ActorType.User, roundTrip.Type);
  }

  [Fact(DisplayName = "GetActorId: it should produce an ActorId that round-trips with a realm.")]
  public void Given_actorWithRealm_When_GetActorIdThenToActor_Then_matchesOriginal()
  {
    Guid realmId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    Actor actor = new Actor { Id = id, Type = ActorType.User, RealmId = realmId };

    ActorId actorId = actor.GetActorId();
    Actor roundTrip = actorId.ToActor();

    Assert.Equal(realmId, roundTrip.RealmId);
    Assert.Equal(id, roundTrip.Id);
    Assert.Equal(ActorType.User, roundTrip.Type);
  }

  [Fact(DisplayName = "ToActor: it should parse actor type case-insensitively from the serialized kind.")]
  public void Given_actorIdWithLowercaseTypeKind_When_ToActor_Then_parsesType()
  {
    Guid id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    Actor original = new Actor { Id = id, Type = ActorType.User, RealmId = null };
    string value = original.GetActorId().Value;
    string[] parts = value.Split(':');
    string lowerKind = parts[0].ToLowerInvariant();
    string rebuilt = string.Join(":", lowerKind, parts[1]);
    ActorId actorId = new ActorId(rebuilt);

    Actor parsed = actorId.ToActor();

    Assert.Equal(ActorType.User, parsed.Type);
    Assert.Equal(id, parsed.Id);
  }

  [Fact(DisplayName = "ToActor: it should throw ArgumentException when the value has more than two pipe-separated segments.")]
  public void Given_actorIdWithTooManySegments_When_ToActor_Then_throwsArgumentException()
  {
    ActorId actorId = new ActorId("a|b|c");

    Assert.Throws<ArgumentException>(() => actorId.ToActor());
  }

  [Fact(DisplayName = "ToActor: it should throw ArgumentException when the actor kind is not a defined ActorType.")]
  public void Given_actorIdWithUnknownTypeKind_When_ToActor_Then_throwsArgumentException()
  {
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    Entity entity = new Entity("NotARealActorType", id);
    ActorId actorId = new ActorId(entity.ToString());

    Assert.Throws<ArgumentException>(() => actorId.ToActor());
  }
}
