using Bogus;
using Logitar;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class EntityTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new entity from arguments.")]
  public void Given_Arguments_When_ctor_Then_Entity()
  {
    string kind = Region.EntityKind;
    Guid id = Guid.NewGuid();
    WorldId worldId = WorldId.NewId();
    long size = _faker.Random.Int(0, 999);

    Entity entity = new(kind, id, worldId, size);

    Assert.Equal(kind, entity.Kind);
    Assert.Equal(id, entity.Id);
    Assert.Equal(worldId, entity.WorldId);
    Assert.Equal(size, entity.Size);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the size is negative.")]
  public void Given_NegatizeSize_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Entity(World.EntityKind, Guid.NewGuid(), worldId: null, size: -1));
    Assert.Equal("size", exception.ParamName);
  }

  [Theory(DisplayName = "Parse: it should parse the entity string.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_ValidString_When_Parse_Then_EntityParsed(bool hasWorldId)
  {
    string kind = Region.EntityKind;
    Guid id = Guid.NewGuid();
    WorldId? worldId = hasWorldId ? WorldId.NewId() : null;

    string entityValue = string.Join(':', kind, Convert.ToBase64String(id.ToByteArray()).ToUriSafeBase64());
    string value = worldId.HasValue ? string.Join('|', worldId, entityValue) : entityValue;
    Entity entity = Entity.Parse(value);

    Assert.Equal(kind, entity.Kind);
    Assert.Equal(id, entity.Id);
    Assert.Equal(worldId, entity.WorldId);
  }

  [Fact(DisplayName = "Parse: it should throw ArgumentException when the entity is not valid.")]
  public void Given_InvalidEntity_When_Parse_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => Entity.Parse("a:b:c"));
    Assert.Equal("value", exception.ParamName);
  }

  [Fact(DisplayName = "Parse: it should throw ArgumentException when the entity kind was not expected.")]
  public void Given_UnexpectedKind_When_Parse_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => Entity.Parse("invalid:123", World.EntityKind));
    Assert.Equal("value", exception.ParamName);
  }

  [Fact(DisplayName = "Parse: it should throw ArgumentException when the value is not valid.")]
  public void Given_InvalidValue_When_Parse_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => Entity.Parse("a|b|c"));
    Assert.Equal("value", exception.ParamName);
  }

  [Theory(DisplayName = "ToString: it should return the correct value.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_Entity_When_ToString_Then_CorrectValue(bool hasWorldId)
  {
    string kind = Region.EntityKind;
    Guid id = Guid.NewGuid();
    WorldId? worldId = hasWorldId ? WorldId.NewId() : null;
    long size = _faker.Random.Int(0, 999);

    Entity entity = new(kind, id, worldId, size);

    string entityValue = string.Join(':', kind, Convert.ToBase64String(id.ToByteArray()).ToUriSafeBase64());
    string expectedValue = worldId.HasValue ? string.Join('|', worldId.Value, entityValue) : entityValue;
    Assert.Equal(expectedValue, entity.ToString());
  }
}
