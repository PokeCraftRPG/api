using Bogus;

namespace PokeGame.Core.Forms;

[Trait(Traits.Category, Categories.Unit)]
public class HeightTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Height.")]
  public void Given_ValidValue_When_ctor_Then_Height()
  {
    int value = _faker.Random.Int(1, 1000);
    Height height = new(value);
    Assert.Equal(value, height.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Height(0));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Height_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(1, 1000);
    Height height = new(value);
    Assert.Equal(value.ToString(), height.ToString());
  }
}
