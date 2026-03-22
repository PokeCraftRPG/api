using FluentValidation;

namespace PokeGame.Core.Validation;

[Trait(Traits.Category, Categories.Unit)]
public class SlugValidatorTests
{
  private readonly ValidationContext<SlugValidatorTests> _context;
  private readonly SlugValidator<SlugValidatorTests> _validator = new();

  public SlugValidatorTests()
  {
    _context = new ValidationContext<SlugValidatorTests>(this);
  }

  [Theory(DisplayName = "IsValid: it should return false when the value is not valid.")]
  [InlineData("")]
  [InlineData("  ")]
  [InlineData("invalid-")]
  [InlineData("-invalid")]
  [InlineData("in--valid")]
  [InlineData("inval!d")]
  public void Given_InvalidValue_When_IsValid_Then_FalseReturned(string value)
  {
    Assert.False(_validator.IsValid(_context, value));
  }

  [Theory(DisplayName = "IsValid: it should return true when the value is valid.")]
  [InlineData("valid")]
  [InlineData("the-new-world-123")]
  public void Given_ValidValue_When_IsValid_Then_TrueReturned(string value)
  {
    Assert.True(_validator.IsValid(_context, value));
  }
}
