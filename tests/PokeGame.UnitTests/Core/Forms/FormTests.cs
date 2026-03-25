using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class FormTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public FormTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the form and variety are not in the same world.")]
  public void Given_WorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu();
    FormId formId = FormId.NewId(_world.Id);

    var exception = Assert.Throws<WorldMismatchException>(() => new Form(variety, isDefault: true, variety.Key, _world.OwnerId, formId));
    Assert.Equal(formId.GetEntity(), exception.Expected);
    Assert.Equal(variety.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("variety", exception.ParamName);
  }
}
