using FluentValidation;

namespace PokeGame.Core.Validation;

[Trait(Traits.Category, Categories.Unit)]
public class UrlValidatorTests
{
  private readonly ValidationContext<UrlValidatorTests> _context;
  private readonly UrlValidator<UrlValidatorTests> _validator = new();

  public UrlValidatorTests()
  {
    _context = new ValidationContext<UrlValidatorTests>(this);
  }

  [Theory(DisplayName = "IsValid: it should return false when the value is not valid.")]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData("invalid")]
  [InlineData("inval.id")]
  public void Given_InvalidValue_When_IsValid_Then_FalseReturned(string value)
  {
    Assert.False(_validator.IsValid(_context, value));
  }

  [Theory(DisplayName = "IsValid: it should return true when the value is valid.")]
  [InlineData("http://test.com")]
  [InlineData("https://www.test.com")]
  public void Given_ValidValue_When_IsValid_Then_TrueReturned(string value)
  {
    Assert.True(_validator.IsValid(_context, value));
  }
}
