using Bogus;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class NumberTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Number.")]
  public void Given_ValidValue_When_ctor_Then_Number()
  {
    int value = _faker.Random.Int(Number.MinimumValue, Number.MaximumValue);
    Number number = new(value);
    Assert.Equal(value, number.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  [InlineData(0)]
  [InlineData(10000)]
  public void Given_Invalid_When_ctor_Then_ValidationException(int value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Number(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Number_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(Number.MinimumValue, Number.MaximumValue);
    Number number = new(value);
    Assert.Equal(value.ToString(), number.ToString());
  }
}
