using Bogus;

namespace PokeGame.Core;

[Trait(Traits.Category, Categories.Unit)]
public class UrlTests
{
  private readonly Faker _faker = new();

  [Fact(DisplayName = "ctor: it should create a new Url.")]
  public void Given_ValidValue_When_ctor_Then_Url()
  {
    string value = string.Concat("  ", _faker.Internet.Url(), "  ");
    Url url = new(value);
    Assert.Equal(value.Trim(), url.Value);
  }

  [Fact(DisplayName = "ctor: it should throw ValidationException when the value is too long.")]
  public void Given_TooLong_When_ctor_Then_ValidationException()
  {
    string value = string.Join('/', _faker.Internet.Url(), _faker.Random.String(Url.MaximumLength, 'a', 'z'));
    var exception = Assert.Throws<FluentValidation.ValidationException>(() => new Url(value));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Value");
  }

  [Fact(DisplayName = "Size: it should return the correct size.")]
  public void Given_Url_When_Size_Then_CorrectValue()
  {
    string value = string.Concat("  ", _faker.Internet.Url(), "  ");
    long size = value.Trim().Length;
    Assert.Equal(size, new Url(value).Size);
  }

  [Fact(DisplayName = "ToString: it should return the correct value.")]
  public void Given_Url_When_ToString_Then_CorrectValue()
  {
    Url url = new(_faker.Internet.Url());
    Assert.Equal(url.Value, url.ToString());
  }

  [Fact(DisplayName = "TryCreate: it should return a Url when the value is valid.")]
  public void Given_ValidValue_When_TryCreate_Then_Url()
  {
    string value = string.Concat("  ", _faker.Internet.Url(), "  ");
    Url? url = Url.TryCreate(value);
    Assert.NotNull(url);
    Assert.Equal(value.Trim(), url.Value);
  }

  [Theory(DisplayName = "TryCreate: it should return null when the value is empty.")]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("  ")]
  public void Given_EmptyValue_When_TryCreate_Then_NullReturned(string? value)
  {
    Assert.Null(Url.TryCreate(value));
  }

  // TODO(fpion): ValidationException when the value is null, empty or white-space
  // TODO(fpion): ValidationException when the value is not a valid URL
}
