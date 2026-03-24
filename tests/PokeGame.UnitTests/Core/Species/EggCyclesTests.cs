using Bogus;

namespace PokeGame.Core.Species;

[Trait(Traits.Category, Categories.Unit)]
public class EggCyclesTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new EggCycles.")]
  public void Given_ValidValue_When_ctor_Then_EggCycles()
  {
    byte value = _faker.Random.Byte(min: 1);
    EggCycles eggCycles = new(value);
    Assert.Equal(value, eggCycles.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new EggCycles(0));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_EggCycles_When_ToString_Then_CorrectValue()
  {
    byte value = _faker.Random.Byte(min: 1);
    EggCycles eggCycles = new(value);
    Assert.Equal(value.ToString(), eggCycles.ToString());
  }
}
