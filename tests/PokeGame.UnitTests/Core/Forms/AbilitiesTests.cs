using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class AbilitiesTests
{
  private readonly Faker _faker = new();
  private readonly World _world;

  public AbilitiesTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "It should construct Abilities from valid arguments.")]
  public void Given_ValidArguments_When_ctor_Then_Abilities()
  {
    Ability @static = AbilityBuilder.Static(_faker, _world);
    Ability lightningRod = AbilityBuilder.LightningRod(_faker, _world);
    Ability surgeSurfer = AbilityBuilder.SurgeSurfer(_faker, _world);
    Abilities abilities = new(@static, surgeSurfer, lightningRod);
    Assert.Equal(@static.Id, abilities.Primary);
    Assert.Equal(surgeSurfer.Id, abilities.Secondary);
    Assert.Equal(lightningRod.Id, abilities.Hidden);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    AbilityId abilityId = new();
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Abilities(abilityId, abilityId, abilityId));
    Assert.Equal(7, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Primary.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Secondary.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Hidden.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Secondary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEqualValidator" && e.PropertyName == "Hidden");
  }
}
