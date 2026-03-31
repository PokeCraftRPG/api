using Bogus;

namespace PokeGame.Core.Items;

[Trait(Traits.Category, Categories.Unit)]
public class PriceTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Price.")]
  public void Given_ValidValue_When_ctor_Then_Price()
  {
    int value = _faker.Random.Int(1, 999999);
    Price price = new(value);
    Assert.Equal(value, price.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  [InlineData(-999)]
  [InlineData(0)]
  public void Given_Invalid_When_ctor_Then_ValidationException(int value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Price(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Price_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(1, 999999);
    Price price = new(value);
    Assert.Equal(value.ToString(), price.ToString());
  }
}
