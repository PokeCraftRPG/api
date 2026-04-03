using Bogus;

namespace PokeGame.Core.Regions;

[Trait(Traits.Category, Categories.Unit)]
public class LocationTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Location.")]
  public void Given_ValidValue_When_ctor_Then_Location()
  {
    string value = "  Mt. Coronet  ";
    Location location = new(value);
    Assert.Equal(value.Trim(), location.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is empty.")]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Location(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(Location.MaximumLength + 1);
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Location(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Location_When_Size_Then_CorrectValue()
  {
    string value = "  Mt. Coronet  ";
    long size = value.Trim().Length;
    Assert.Equal(size, new Location(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Location_When_ToString_Then_CorrectValue()
  {
    Location location = new("Mt. Coronet");
    Assert.Equal(location.Value, location.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Location when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Location()
  {
    string value = "  Mt. Coronet  ";
    Location? location = Location.TryCreate(value);
    Assert.NotNull(location);
    Assert.Equal(value.Trim(), location.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Location.TryCreate(value));
  }
}
