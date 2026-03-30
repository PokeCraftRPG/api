using FluentValidation;
using FluentValidation.Results;
using Krakenar.Contracts.Settings;

namespace PokeGame.Core.Validation;

[Trait(Traits.Category, Categories.Unit)]
public class PasswordValidatorTests
{
  private readonly PasswordSettings _passwordSettings = new();
  private readonly PasswordValidator _validator;

  public PasswordValidatorTests()
  {
    _validator = new PasswordValidator(_passwordSettings);
  }

  [Fact(DisplayName = "Validate: it should return a valid result when the password is valid.")]
  public void Given_ValidPassword_When_Validate_Then_ValidResult()
  {
    ValidationResult result = _validator.Validate("Test123!");
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }

  [Fact(DisplayName = "Validate: it should return an invalid result when the password is not valid.")]
  public void Given_InvalidPassword_When_Validate_Then_InvalidResult()
  {
    ValidationResult result = _validator.Validate("invalid");
    Assert.False(result.IsValid);
    Assert.Equal(5, result.Errors.Count);
    Assert.Contains(result.Errors, e => e.ErrorCode == "PasswordTooShort");
    Assert.Contains(result.Errors, e => e.ErrorCode == "PasswordRequiresUniqueChars");
    Assert.Contains(result.Errors, e => e.ErrorCode == "PasswordRequiresNonAlphanumeric");
    Assert.Contains(result.Errors, e => e.ErrorCode == "PasswordRequiresUpper");
    Assert.Contains(result.Errors, e => e.ErrorCode == "PasswordRequiresDigit");
  }

  private class PasswordValidator : AbstractValidator<string>
  {
    public PasswordValidator(IPasswordSettings passwordSettings)
    {
      RuleFor(x => x).Password(passwordSettings);
    }
  }
}
