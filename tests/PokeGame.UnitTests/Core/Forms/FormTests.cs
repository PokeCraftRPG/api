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

    Height height = new(4);
    Weight weight = new(60);
    FormTypes types = new(PokemonType.Electric);
    Sprites sprites = new(
      new Url("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png"),
      new Url("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png"));

    var exception = Assert.Throws<WorldMismatchException>(() => new Form(variety, isDefault: true, variety.Key, height, weight, types, sprites, _world.OwnerId, formId));
    Assert.Equal(formId.GetEntity(), exception.Expected);
    Assert.Equal(variety.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("variety", exception.ParamName);
  }
}
