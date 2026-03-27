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

  [Fact(DisplayName = "Abilities: it should throw WorldMismatchException when the form and abilities are not in the same world.")]
  public void Given_AbilityWorldMismatch_When_Abilities_Then_WorldMismatchException()
  {
    Form form = FormBuilder.Raichu(_faker, _world);

    World world = new WorldBuilder(_faker).Build();
    Abilities abilities = new(AbilityBuilder.Static(_faker, world), AbilityBuilder.SurgeSurfer(_faker, world), AbilityBuilder.LightningRod(_faker, world));

    var exception = Assert.Throws<WorldMismatchException>(() => form.Abilities = abilities);
    Assert.Equal(form.Id.GetEntity(), exception.Expected);
    Assert.True(abilities.Secondary.HasValue);
    Assert.True(abilities.Hidden.HasValue);
    Assert.Equal([abilities.Primary.GetEntity(), abilities.Secondary.Value.GetEntity(), abilities.Hidden.Value.GetEntity()], exception.Mismatched);
    Assert.Equal("Abilities", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the form and abilities are not in the same world.")]
  public void Given_AbilityWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    World world = new WorldBuilder(_faker).Build();

    Variety variety = VarietyBuilder.Pikachu(_faker, _world);
    Height height = new(4);
    Weight weight = new(60);
    Types types = new(PokemonType.Electric);
    Abilities abilities = new(AbilityBuilder.Static(_faker, world), AbilityBuilder.SurgeSurfer(_faker, world), AbilityBuilder.LightningRod(_faker, world));
    BaseStatistics baseStatistics = new(35, 55, 40, 50, 50, 90);
    Yield yield = new(112, 0, 0, 0, 0, 0, 2);
    Sprites sprites = new(
      new Url("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png"),
      new Url("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png"));
    FormId formId = FormId.NewId(_world.Id);

    var exception = Assert.Throws<WorldMismatchException>(
      () => new Form(variety, isDefault: true, variety.Key, height, weight, types, abilities, baseStatistics, yield, sprites, _world.OwnerId, formId));
    Assert.Equal(formId.GetEntity(), exception.Expected);
    Assert.True(abilities.Secondary.HasValue);
    Assert.True(abilities.Hidden.HasValue);
    Assert.Equal([abilities.Primary.GetEntity(), abilities.Secondary.Value.GetEntity(), abilities.Hidden.Value.GetEntity()], exception.Mismatched);
    Assert.Equal("abilities", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the form and variety are not in the same world.")]
  public void Given_VarietyWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu();
    FormId formId = FormId.NewId(_world.Id);

    Height height = new(4);
    Weight weight = new(60);
    Types types = new(PokemonType.Electric);
    Abilities abilities = new(AbilityBuilder.Static(), secondary: null, AbilityBuilder.LightningRod());
    BaseStatistics baseStatistics = new(35, 55, 40, 50, 50, 90);
    Yield yield = new(112, 0, 0, 0, 0, 0, 2);
    Sprites sprites = new(
      new Url("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png"),
      new Url("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png"));

    var exception = Assert.Throws<WorldMismatchException>(
      () => new Form(variety, isDefault: true, variety.Key, height, weight, types, abilities, baseStatistics, yield, sprites, _world.OwnerId, formId));
    Assert.Equal(formId.GetEntity(), exception.Expected);
    Assert.Equal(variety.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("variety", exception.ParamName);
  }
}
