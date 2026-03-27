using Bogus;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class SlugTests
{
  private const string SlugValue = "the-new-world";

  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Slug.")]
  public void Given_ValidValue_When_ctor_Then_Slug()
  {
    string value = string.Concat("  ", SlugValue.ToUpperInvariant(), "  ");
    Slug slug = new(value);
    Assert.Equal(SlugValue, slug.Value);
  }

  [Theory(DisplayName = "ctor: it should throw ValidationException when the value is empty.")]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_Empty_When_ctor_Then_ValidationException(string value)
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Slug(value));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is not valid.")]
  public void Given_Invalid_When_ctor_Then_ValidationException()
  {
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Slug("invalid--value!"));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = _faker.Random.String(Slug.MaximumLength + 1, 'a', 'z');
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Slug(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Normalize: it should return the correct value.")]
  public void Given_Value_When_Normalize_Then_Normalized()
  {
    string value = string.Concat("  ", SlugValue.ToUpperInvariant(), "  ");
    Assert.Equal(SlugValue, Slug.Normalize(value));
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Slug_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", SlugValue, "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Slug(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Slug_When_ToString_Then_CorrectValue()
  {
    Slug slug = new(SlugValue);
    Assert.Equal(slug.Value, slug.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Slug when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Slug()
  {
    string value = string.Concat("  ", SlugValue, "  ");
    Slug? slug = Slug.TryCreate(value);
    Assert.NotNull(slug);
    Assert.Equal(value.Trim(), slug.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Slug.TryCreate(value));
  }
}
