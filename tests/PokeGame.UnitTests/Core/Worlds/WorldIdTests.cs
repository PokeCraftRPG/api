namespace PokeGame.Core.Worlds;

[Trait(Traits.Category, Categories.Unit)]
public class WorldIdTests
{
  [Fact(DisplayName = "ToGuid: it should preserve Value when round-tripping through a new WorldId from the guid.")]
  public void Given_newId_When_ToGuid_Then_matchesUnderlyingGuid()
  {
    WorldId worldId = WorldId.NewId();

    Guid roundTripped = worldId.ToGuid();

    WorldId again = new WorldId(roundTripped);
    Assert.Equal(worldId.Value, again.Value);
  }

  [Fact(DisplayName = "ctor: it should produce Value that parses as a World Entity stream id with the same guid.")]
  public void Given_guid_When_ctor_Then_ValueParsesAsWorldEntity()
  {
    Guid id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    WorldId worldId = new WorldId(id);

    Entity entity = Entity.Parse(worldId.Value, World.EntityKind);
    Assert.Equal(World.EntityKind, entity.Kind);
    Assert.Equal(id, entity.Id);
  }

  [Fact(DisplayName = "ctor: it should preserve StreamId Value when constructed from an existing WorldId string.")]
  public void Given_existingStreamValue_When_ctor_Then_ValueMatches()
  {
    WorldId original = WorldId.NewId();

    WorldId copy = new WorldId(original.Value);

    Assert.Equal(original.Value, copy.Value);
  }

  [Fact(DisplayName = "==: it should yield true for equal values with == and Equals, false for !=, and matching GetHashCode.")]
  public void Given_sameValue_When_comparing_Then_areEqual()
  {
    WorldId original = WorldId.NewId();
    WorldId same = new WorldId(original.Value);

    Assert.True(original == same);
    Assert.False(original != same);
    Assert.True(original.Equals(same));
    Assert.Equal(original.GetHashCode(), same.GetHashCode());
  }

  [Fact(DisplayName = "==: it should yield false for == and Equals and true for != when values differ.")]
  public void Given_differentValues_When_comparing_Then_areNotEqual()
  {
    WorldId left = WorldId.NewId();
    WorldId right = WorldId.NewId();

    Assert.False(left == right);
    Assert.True(left != right);
    Assert.False(left.Equals(right));
  }

  [Fact(DisplayName = "Equals: it should return false for null and for a non-WorldId value.")]
  public void Given_nullOrOtherType_When_Equals_Then_returnsFalse()
  {
    WorldId worldId = WorldId.NewId();

    Assert.False(worldId.Equals(null));
    Assert.False(worldId.Equals(worldId.Value));
  }

  [Fact(DisplayName = "ToString: it should return the same text as Value.")]
  public void Given_worldId_When_ToString_Then_matchesValue()
  {
    WorldId worldId = WorldId.NewId();

    Assert.Equal(worldId.Value, worldId.ToString());
  }
}
