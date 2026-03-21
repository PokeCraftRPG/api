using PokeGame.Core.Worlds;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class EntityTests
{
  [Fact(DisplayName = "Entity constructor: it should assign Kind, Id, WorldId, and Size.")]
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

  [Fact(DisplayName = "Entity constructor: it should reject a negative Size.")]
  public void Given_negativeSize_When_ctor_Then_throwsArgumentOutOfRangeException()
  {
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      new Entity("Kind", Guid.NewGuid(), size: -1L));
  }

  [Fact(DisplayName = "Entity.Parse: it should round-trip ToString output without WorldId.")]
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

  [Fact(DisplayName = "Entity.Parse: it should round-trip ToString output with WorldId.")]
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

  [Fact(DisplayName = "Entity.Parse: it should accept a matching expectedKind.")]
  public void Given_roundTrippedValueAndExpectedKind_When_Parse_Then_succeeds()
  {
    Entity original = new Entity("Item", Guid.NewGuid());
    string serialized = original.ToString();

    Entity parsed = Entity.Parse(serialized, expectedKind: "Item");

    Assert.Equal(original.Kind, parsed.Kind);
    Assert.Equal(original.Id, parsed.Id);
  }

  [Fact(DisplayName = "Entity.Parse: it should reject when expectedKind does not match.")]
  public void Given_roundTrippedValueAndWrongExpectedKind_When_Parse_Then_throwsArgumentException()
  {
    Entity original = new Entity("Item", Guid.NewGuid());
    string serialized = original.ToString();

    Assert.Throws<ArgumentException>(() => Entity.Parse(serialized, expectedKind: "Other"));
  }

  [Theory(DisplayName = "Entity.Parse: it should reject malformed serialized values.")]
  [InlineData("a|b|c")]
  [InlineData("no-colon-segment")]
  [InlineData("prefix|still-no-colon")]
  public void Given_malformedSerializedEntity_When_Parse_Then_throwsArgumentException(string value)
  {
    Assert.Throws<ArgumentException>(() => Entity.Parse(value));
  }

  [Fact(DisplayName = "Entity.Parse: it should reject a Kind segment that is not valid URI-safe Base64 for a Guid.")]
  public void Given_invalidGuidEncoding_When_Parse_Then_throws()
  {
    Assert.ThrowsAny<Exception>(() => Entity.Parse("Kind:not-valid-base64!!!"));
  }

  [Fact(DisplayName = "Entity.ToString: it should omit Size so Parse does not restore it.")]
  public void Given_entityWithSize_When_ToStringThenParse_Then_SizeIsNull()
  {
    Entity original = new Entity("Buff", Guid.NewGuid(), size: 99L);
    Entity parsed = Entity.Parse(original.ToString());
    Assert.Null(parsed.Size);
  }
}
