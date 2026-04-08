using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class EffortValuesTests
{
  private readonly Faker _faker = new();
  private readonly IPokemonRandomizer _randomizer = PokemonRandomizer.Instance;

  [Fact(DisplayName = "It should create an instance from another.")]
  public void Given_Instance_When_ctor_Then_Instance()
  {
    EffortValues instance = new(252, 18, 128, 32, 64, 16);
    EffortValues effortValues = new(instance);
    Assert.Equal(instance.HP, effortValues.HP);
    Assert.Equal(instance.Attack, effortValues.Attack);
    Assert.Equal(instance.Defense, effortValues.Defense);
    Assert.Equal(instance.SpecialAttack, effortValues.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, effortValues.SpecialDefense);
    Assert.Equal(instance.Speed, effortValues.Speed);
  }

  [Fact(DisplayName = "It should create an instance from arguments.")]
  public void Given_Arguments_When_ctor_Then_Instance()
  {
    EffortValues instance = new(64, 128, 18, 32, 252, 16);
    EffortValues effortValues = new(instance.HP, instance.Attack, instance.Defense, instance.SpecialAttack, instance.SpecialDefense, instance.Speed);
    Assert.Equal(instance.HP, effortValues.HP);
    Assert.Equal(instance.Attack, effortValues.Attack);
    Assert.Equal(instance.Defense, effortValues.Defense);
    Assert.Equal(instance.SpecialAttack, effortValues.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, effortValues.SpecialDefense);
    Assert.Equal(instance.Speed, effortValues.Speed);
  }

  [Fact(DisplayName = "It should create the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    EffortValues effortValues = new();
    Assert.Equal(0, effortValues.HP);
    Assert.Equal(0, effortValues.Attack);
    Assert.Equal(0, effortValues.Defense);
    Assert.Equal(0, effortValues.SpecialAttack);
    Assert.Equal(0, effortValues.SpecialDefense);
    Assert.Equal(0, effortValues.Speed);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new EffortValues(255, 0, 255, 0, 255, 0));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EffortValuesValidator");
  }
}
