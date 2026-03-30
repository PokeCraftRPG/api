using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountPayload
{
  public string Locale { get; set; } = string.Empty;
  public Credentials? Credentials { get; set; }
  public string? Token { get; set; }
  public OneTimePasswordValidation? OneTimePassword { get; set; }
  public CompleteProfilePayload? Profile { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SignInAccountPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Locale).Locale();
      When(x => x.Credentials is not null, () => RuleFor(x => x.Credentials!).SetValidator(new CredentialsValidator()));
      When(x => x.OneTimePassword is not null, () => RuleFor(x => x.OneTimePassword!).SetValidator(new OneTimePasswordValidationValidator()));
      When(x => x.Profile is not null, () => RuleFor(x => x.Profile!).SetValidator(new CompleteProfilePayloadValidator()));

      // TODO(fpion): exactly 1 property.
    }
  }
}
