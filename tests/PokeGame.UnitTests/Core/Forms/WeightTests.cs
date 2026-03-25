using Bogus;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class WeightTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Weight.")]
  public void Given_ValidValue_When_ctor_Then_Weight()
  {
    int value = _faker.Random.Int(1, 9999);
    Weight weight = new(value);
    Assert.Equal(value, weight.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Weight(0));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Weight_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(1, 1000);
    Weight weight = new(value);
    Assert.Equal(value.ToString(), weight.ToString());
  }
}
