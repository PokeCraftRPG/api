using FluentValidation;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountPayload
{
  public Credentials? Credentials { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SignInAccountPayload>
  {
    public Validator()
    {
      When(x => x.Credentials is not null, () => RuleFor(x => x.Credentials!).SetValidator(new CredentialsValidator()));
    }
  }
}
