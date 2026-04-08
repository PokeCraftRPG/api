using FluentValidation;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountPayload
{
  public Credentials? Credentials { get; set; }
  public string? AuthenticationToken { get; set; }
  public OneTimePasswordValidation? OneTimePassword { get; set; }
  public CompleteProfilePayload? Profile { get; set; }

  [JsonPropertyName("refresh_token")]
  public string? RefreshToken { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SignInAccountPayload>
  {
    public Validator()
    {
      RuleFor(x => x).Must(BeValid)
        .WithErrorCode("SignInAccountValidator")
        .WithMessage(p =>
        {
          string[] properties = [nameof(p.Credentials), nameof(p.AuthenticationToken), nameof(p.OneTimePassword), nameof(p.Profile), nameof(p.RefreshToken)];
          return $"Exactly one of the following must be specified: {string.Join(", ", properties)}.";
        });
    }

    private static bool BeValid(SignInAccountPayload payload)
    {
      int count = 0;
      if (payload.Credentials is not null)
      {
        count++;
      }
      if (payload.AuthenticationToken is not null)
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
      if (payload.RefreshToken is not null)
      {
        count++;
      }
      return count == 1;
    }
  }
}
