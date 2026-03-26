using FluentValidation;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class TypesTests
{
  [Fact(DisplayName = "It should create an empty instance.")]
  public void Given_Empty_When_ctor_Then_Default()
  {
    Types types = new();
    Assert.Equal(default, types.Primary);
    Assert.Null(types.Secondary);
  }

  [Fact(DisplayName = "It should create Types from another instance.")]
  public void Given_Instance_When_ctor_Then_Types()
  {
    Types instance = new(PokemonType.Electric, PokemonType.Fairy);
    Types types = new(instance);
    Assert.Equal(instance.Primary, types.Primary);
    Assert.Equal(instance.Secondary, types.Secondary);
  }

  [Theory(DisplayName = "It should create Types from arguments.")]
  [InlineData(PokemonType.Grass, PokemonType.Poison)]
  [InlineData(PokemonType.Fire)]
  public void Given_ValidArguments_When_ctor_Then_Types(PokemonType primary, PokemonType? secondary = null)
  {
    Types types = new(primary, secondary);
    Assert.Equal(primary, types.Primary);
    Assert.Equal(secondary, types.Secondary);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    PokemonType type = (PokemonType)(-1);
    var exception = Assert.Throws<ValidationException>(() => new Types(type, type));

    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Primary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Secondary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Secondary");
  }
}
