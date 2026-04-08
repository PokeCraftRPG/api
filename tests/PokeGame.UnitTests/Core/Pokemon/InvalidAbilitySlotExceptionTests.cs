using PokeGame.Builders;
using PokeGame.Core.Abilities;
using FormAbilities = PokeGame.Core.Forms.Abilities;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class InvalidAbilitySlotExceptionTests
{
  private const string PropertyName = "AbilitySlot";

  [Fact(DisplayName = "It should not throw when the ability slot is valid.")]
  public void Given_Valid_When_ThrowIfNotValid_Then_Succeed()
  {
    FormAbilities abilities = new(AbilityBuilder.Static(), AbilityBuilder.SurgeSurfer(), AbilityBuilder.LightningRod());

    foreach (AbilitySlot slot in Enum.GetValues<AbilitySlot>())
    {
      InvalidAbilitySlotException.ThrowIfNotValid(abilities, slot, PropertyName);
    }
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when the ability slot is not defined.")]
  public void Given_NotDefined_When_ThrowIfNotValid_Then_ArgumentOutOfRangeException()
  {
    FormAbilities abilities = new(AbilityBuilder.Static(), AbilityBuilder.SurgeSurfer(), AbilityBuilder.LightningRod());

    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => InvalidAbilitySlotException.ThrowIfNotValid(abilities, (AbilitySlot)999, PropertyName));
    Assert.Equal("slot", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw InvalidAbilitySlotException when the form does not have a Hidden ability.")]
  public void Given_NoHidden_When_ThrowIfNotValid_Then_InvalidAbilitySlotException()
  {
    FormAbilities abilities = new(AbilityBuilder.Static());
    AbilitySlot slot = AbilitySlot.Hidden;

    var exception = Assert.Throws<InvalidAbilitySlotException>(() => InvalidAbilitySlotException.ThrowIfNotValid(abilities, slot, PropertyName));
    Assert.Equal(slot, exception.AbilitySlot);
    Assert.Equal(PropertyName, exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw InvalidAbilitySlotException when the form does not have a Secondary ability.")]
  public void Given_NoSecondary_When_ThrowIfNotValid_Then_InvalidAbilitySlotException()
  {
    FormAbilities abilities = new(AbilityBuilder.Static());
    AbilitySlot slot = AbilitySlot.Secondary;

    var exception = Assert.Throws<InvalidAbilitySlotException>(() => InvalidAbilitySlotException.ThrowIfNotValid(abilities, slot, PropertyName));
    Assert.Equal(slot, exception.AbilitySlot);
    Assert.Equal(PropertyName, exception.PropertyName);
  }
}
