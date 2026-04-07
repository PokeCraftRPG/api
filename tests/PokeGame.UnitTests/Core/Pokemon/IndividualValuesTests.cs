using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class IndividualValuesTests
{
  private readonly Faker _faker = new();
  private readonly IPokemonRandomizer _randomizer = PokemonRandomizer.Instance;

  [Fact(DisplayName = "It should create an instance from another.")]
  public void Given_Instance_When_ctor_Then_Instance()
  {
    IndividualValues instance = _randomizer.IndividualValues();
    IndividualValues individualValues = new(instance);
    Assert.Equal(instance.HP, individualValues.HP);
    Assert.Equal(instance.Attack, individualValues.Attack);
    Assert.Equal(instance.Defense, individualValues.Defense);
    Assert.Equal(instance.SpecialAttack, individualValues.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, individualValues.SpecialDefense);
    Assert.Equal(instance.Speed, individualValues.Speed);
  }

  [Fact(DisplayName = "It should create an instance from arguments.")]
  public void Given_Arguments_When_ctor_Then_Instance()
  {
    IndividualValues instance = _randomizer.IndividualValues();
    IndividualValues individualValues = new(instance.HP, instance.Attack, instance.Defense, instance.SpecialAttack, instance.SpecialDefense, instance.Speed);
    Assert.Equal(instance.HP, individualValues.HP);
    Assert.Equal(instance.Attack, individualValues.Attack);
    Assert.Equal(instance.Defense, individualValues.Defense);
    Assert.Equal(instance.SpecialAttack, individualValues.SpecialAttack);
    Assert.Equal(instance.SpecialDefense, individualValues.SpecialDefense);
    Assert.Equal(instance.Speed, individualValues.Speed);
  }

  [Fact(DisplayName = "It should create the default instance.")]
  public void Given_NoArgument_When_ctor_Then_DefaultInstance()
  {
    IndividualValues individualValues = new();
    Assert.Equal(0, individualValues.HP);
    Assert.Equal(0, individualValues.Attack);
    Assert.Equal(0, individualValues.Defense);
    Assert.Equal(0, individualValues.SpecialAttack);
    Assert.Equal(0, individualValues.SpecialDefense);
    Assert.Equal(0, individualValues.Speed);
  }

  [Fact(DisplayName = "It should throw ValidationException when the arguments are not valid.")]
  public void Given_InvalidArguments_When_ctor_Then_ValidationException()
  {
    byte[] bytes = new byte[6];
    for (int i = 0; i < bytes.Length; i++)
    {
      bytes[i] = _faker.Random.Byte(IndividualValues.MaximumValue + 1);
    }

    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new IndividualValues(bytes[0], bytes[1], bytes[2], bytes[3], bytes[5], bytes[5]));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "HP");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "SpecialAttack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "SpecialDefense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "LessThanOrEqualValidator" && e.PropertyName == "Speed");
  }
}
