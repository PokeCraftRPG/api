using FluentValidation;

namespace PokeGame.Core.Validation;

[Trait(Traits.Category, Categories.Unit)]
public class GenderValidatorTests
{
  private readonly ValidationContext<GenderValidatorTests> _context;
  private readonly GenderValidator<GenderValidatorTests> _validator = new();

  public GenderValidatorTests()
  {
    _context = new ValidationContext<GenderValidatorTests>(this);
  }

  [Theory(DisplayName = "IsValid: it should return false when the value is not known.")]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("unknown")]
  public void Given_UnknownValue_When_IsValid_Then_FalseReturned(string value)
  {
    Assert.False(_validator.IsValid(_context, value));
  }

  [Theory(DisplayName = "IsValid: it should return true when the value is known.")]
  [InlineData("fEmAlE")]
  [InlineData("male")]
  [InlineData(" other  ")]
  public void Given_KnownValue_When_IsValid_Then_TrueReturned(string value)
  {
    Assert.True(_validator.IsValid(_context, value));
  }
}
