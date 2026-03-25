using Bogus;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class CatchRateTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new CatchRate.")]
  public void Given_ValidValue_When_ctor_Then_CatchRate()
  {
    byte value = _faker.Random.Byte(min: 1);
    CatchRate catchRate = new(value);
    Assert.Equal(value, catchRate.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new CatchRate(0));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_CatchRate_When_ToString_Then_CorrectValue()
  {
    byte value = _faker.Random.Byte(min: 1);
    CatchRate catchRate = new(value);
    Assert.Equal(value.ToString(), catchRate.ToString());
  }
}
