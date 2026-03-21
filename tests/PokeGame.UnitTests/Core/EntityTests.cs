using PokeGame.Core.Worlds;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class EntityTests
{
  [Fact(DisplayName = "ctor: it should store Kind, Id, optional WorldId, and optional Size.")]
  public void Given_parameters_When_ctor_Then_storesProperties()
  {
    Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    WorldId worldId = WorldId.NewId();

    Entity withWorld = new Entity("Thing", id, worldId, size: 10L);
    Assert.Equal("Thing", withWorld.Kind);
    Assert.Equal(id, withWorld.Id);
    Assert.Equal(worldId.Value, withWorld.WorldId?.Value);
    Assert.Equal(10L, withWorld.Size);

    Entity minimal = new Entity("Thing", id);
    Assert.Equal("Thing", minimal.Kind);
    Assert.Equal(id, minimal.Id);
    Assert.Null(minimal.WorldId);
    Assert.Null(minimal.Size);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when Size is negative.")]
  public void Given_negativeSize_When_ctor_Then_throwsArgumentOutOfRangeException()
  {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      new Entity("Kind", Guid.NewGuid(), size: -1L));
  }

  [Fact(DisplayName = "Parse: it should restore kind and id when given ToString output without WorldId.")]
  public void Given_entityWithoutWorld_When_ToStringThenParse_Then_restoresKindAndId()
  {
    Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    Entity original = new Entity("Player", id);

    Entity parsed = Entity.Parse(original.ToString());

    Assert.Equal(original.Kind, parsed.Kind);
    Assert.Equal(original.Id, parsed.Id);
    Assert.Null(parsed.WorldId);
    Assert.Null(parsed.Size);
  }

  [Fact(DisplayName = "Parse: it should restore world id, kind, and id when given ToString output that includes WorldId.")]
  public void Given_entityWithWorld_When_ToStringThenParse_Then_restoresWorldKindAndId()
  {
    Guid id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    WorldId worldId = WorldId.NewId();
    Entity original = new Entity("Npc", id, worldId);

    Entity parsed = Entity.Parse(original.ToString());

    Assert.Equal(original.Kind, parsed.Kind);
    Assert.Equal(original.Id, parsed.Id);
    Assert.Equal(worldId.Value, parsed.WorldId?.Value);
    Assert.Null(parsed.Size);
  }

  [Fact(DisplayName = "Parse: it should succeed when expectedKind matches a round-tripped serialized entity.")]
  public void Given_roundTrippedValueAndExpectedKind_When_Parse_Then_succeeds()
  {
    Entity original = new Entity("Item", Guid.NewGuid());
    string serialized = original.ToString();

    Entity parsed = Entity.Parse(serialized, expectedKind: "Item");

    Assert.Equal(original.Kind, parsed.Kind);
    Assert.Equal(original.Id, parsed.Id);
  }

  [Fact(DisplayName = "Parse: it should throw ArgumentException when expectedKind does not match the serialized kind.")]
  public void Given_roundTrippedValueAndWrongExpectedKind_When_Parse_Then_throwsArgumentException()
  {
    Entity original = new Entity("Item", Guid.NewGuid());
    string serialized = original.ToString();

    Assert.Throws<ArgumentException>(() => Entity.Parse(serialized, expectedKind: "Other"));
  }

  [Theory(DisplayName = "Parse: it should throw ArgumentException when the serialized value is malformed.")]
  [InlineData("a|b|c")]
  [InlineData("no-colon-segment")]
  [InlineData("prefix|still-no-colon")]
  public void Given_malformedSerializedEntity_When_Parse_Then_throwsArgumentException(string value)
  {
    Assert.Throws<ArgumentException>(() => Entity.Parse(value));
  }

  [Fact(DisplayName = "Parse: it should throw when the guid segment is not valid URI-safe base64.")]
  public void Given_invalidGuidEncoding_When_Parse_Then_throws()
  {
    Assert.ThrowsAny<Exception>(() => Entity.Parse("Kind:not-valid-base64!!!"));
  }

  [Fact(DisplayName = "Parse: it should yield null Size after round-trip because ToString does not serialize Size.")]
  public void Given_entityWithSize_When_ToStringThenParse_Then_SizeIsNull()
  {
    Entity original = new Entity("Buff", Guid.NewGuid(), size: 99L);
    Entity parsed = Entity.Parse(original.ToString());
    Assert.Null(parsed.Size);
  }
}
