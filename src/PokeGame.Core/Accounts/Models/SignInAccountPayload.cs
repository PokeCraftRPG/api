using FluentValidation;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountPayload
{
  public Credentials? Credentials { get; set; }
  public string? Token { get; set; }
  public OneTimePasswordValidation? OneTimePassword { get; set; }
  public CompleteProfilePayload? Profile { get; set; }
  // TODO(fpion): RefreshToken

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SignInAccountPayload>
  {
    public Validator()
    {
      RuleFor(x => x).Must(BeValid)
        .WithErrorCode("SignInAccountValidator")
        .WithMessage(x => $"Exactly one of the following must be specified: {string.Join(", ", nameof(x.Credentials), nameof(x.Token), nameof(x.OneTimePassword), nameof(x.Token))}.");
    }

    private static bool BeValid(SignInAccountPayload payload)
    {
      int count = 0;
      if (payload.Credentials is not null)
      {
        count++;
      }
      if (payload.Token is not null)
      {
        count++;
      }
      if (payload.OneTimePassword is not null)
      {
        count++;
      }
      if (payload.Profile is not null)
      {
        count++;
      }
      return count == 1;
    }
  }
}
