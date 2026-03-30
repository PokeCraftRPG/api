using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Accounts.Models;

public record Credentials
{
  public string Locale { get; set; }
  public string EmailAddress { get; set; }
  public string? Password { get; set; }

  public Credentials() : this(string.Empty, string.Empty)
  {
  }

  public Credentials(string locale, string emailAddress, string? password = null)
  {
    Locale = locale;
    EmailAddress = emailAddress;
    Password = password;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<Credentials>
  {
    public Validator()
    {
      RuleFor(x => x.Locale).Locale();
      RuleFor(x => x.EmailAddress).NotEmpty().MaximumLength(byte.MaxValue).EmailAddress();
      When(x => x.Password is not null, () => RuleFor(x => x.Password).NotEmpty());
    }
  }
}
