using Bogus;

namespace PokeGame.Core.Trainers;

[Trait(Traits.Category, Categories.Unit)]
public class MoneyTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Money.")]
  public void Given_ValidValue_When_ctor_Then_Money()
  {
    int value = _faker.Random.Int(0, 9999999);
    Money money = new(value);
    Assert.Equal(value, money.Value);
  }

  [Fact(DisplayName = "ctor: it should create the default instance.")]
  public void Given_NoArgument_When_ctor_Then_Money()
  {
    Money money = new();
    Assert.Equal(0, money.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Money(-1));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanOrEqualValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Money_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(0, 9999999);
    Money money = new(value);
    Assert.Equal(value.ToString(), money.ToString());
  }
}
