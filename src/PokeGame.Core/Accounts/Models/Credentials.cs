using FluentValidation;

namespace PokeGame.Core.Accounts.Models;

public record Credentials
{
  public string EmailAddress { get; set; }
  public string? Password { get; set; }

  public Credentials() : this(string.Empty)
  {
  }

  public Credentials(string emailAddress, string? password = null)
  {
    EmailAddress = emailAddress;
    Password = password;
  }
}

internal class CredentialsValidator : AbstractValidator<Credentials>
{
  public CredentialsValidator()
  {
    RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(byte.MaxValue).EmailAddress();
    When(x => x.Password is not null, () => RuleFor(x => x.Password).NotEmpty());
  }
}
