using Bogus;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class LevelTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Level.")]
  public void Given_ValidValue_When_ctor_Then_Level()
  {
    int value = _faker.Random.Int(Level.MinimumValue, Level.MaximumValue);
    Level level = new(value);
    Assert.Equal(value, level.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  [InlineData(0)]
  [InlineData(101)]
  public void Given_Invalid_When_ctor_Then_ValidationException(int value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Level(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Level_When_ToString_Then_CorrectValue()
  {
    int value = _faker.Random.Int(Level.MinimumValue, Level.MaximumValue);
    Level level = new(value);
    Assert.Equal(value.ToString(), level.ToString());
  }
}
