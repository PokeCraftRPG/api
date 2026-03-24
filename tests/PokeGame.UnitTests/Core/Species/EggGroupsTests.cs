using Bogus;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class EggGroupsTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "It should construct a new EggGroups from another instance.")]
  public void Given_Instance_When_ctor_Then_EggGroups()
  {
    EggGroups instance = _faker.EggGroups();
    EggGroups eggGroups = new(instance);
    Assert.Equal(instance.Primary, eggGroups.Primary);
    Assert.Equal(instance.Secondary, eggGroups.Secondary);
  }

  [Fact(DisplayName = "It should construct a new EggGroups from arguments.")]
  public void Given_Arguments_When_ctor_Then_EggGroups()
  {
    EggGroup primary = EggGroup.Field;
    EggGroup secondary = EggGroup.Fairy;
    EggGroups eggGroups = new(primary, secondary);
    Assert.Equal(primary, eggGroups.Primary);
    Assert.Equal(secondary, eggGroups.Secondary);
  }

  [Theory(DisplayName = "It should throw ValidationException when the groups are not valid.")]
  [InlineData((EggGroup)(-1), null)]
  [InlineData(EggGroup.Field, (EggGroup)(-1))]
  [InlineData(EggGroup.Field, EggGroup.Field)]
  [InlineData(EggGroup.Field, EggGroup.NoEggsDiscovered)]
  [InlineData(EggGroup.Field, EggGroup.Ditto)]
  [InlineData(EggGroup.NoEggsDiscovered, EggGroup.Fairy)]
  [InlineData(EggGroup.Ditto, EggGroup.Fairy)]
  public void Given_Invalid_When_ctor_Then_ValidationException(EggGroup primary, EggGroup? secondary)
  {
    Assert.Throws<FluentValidation.ValidationException>(() => new EggGroups(primary, secondary));
  }
}
