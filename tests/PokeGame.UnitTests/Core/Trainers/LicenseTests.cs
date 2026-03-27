using Bogus;

namespace PokeGame.Core.Trainers;

[Trait(Traits.Category, Categories.Unit)]
public class LicenseTests
{
  private const string LicenseValue = "Q-123456-3";

  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new License.")]
  public void Given_ValidValue_When_ctor_Then_License()
  {
    string value = string.Concat("  ", LicenseValue.ToLowerInvariant(), "  ");
    License license = new(value);
    Assert.Equal(LicenseValue, license.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is empty.")]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new License(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(License.MaximumLength + 1, 'a', 'z');
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new License(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Normalize: it should return the correct value.")]
  public void Given_Value_When_Normalize_Then_Normalized()
  {
    string value = string.Concat("  ", LicenseValue.ToLowerInvariant(), "  ");
    Assert.Equal(LicenseValue, License.Normalize(value));
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_License_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", LicenseValue, "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new License(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_License_When_ToString_Then_CorrectValue()
  {
    License license = new(LicenseValue);
    Assert.Equal(license.Value, license.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a License when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_License()
  {
    string value = string.Concat("  ", LicenseValue, "  ");
    License? license = License.TryCreate(value);
    Assert.NotNull(license);
    Assert.Equal(value.Trim(), license.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(License.TryCreate(value));
  }
}
