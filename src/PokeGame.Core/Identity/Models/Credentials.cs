using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Identity.Models;

public record Credentials
{
  public string Locale { get; set; }
  public string EmailAddress { get; set; }
  public string? Password { get; set; }
  public bool UsePasswordless { get; set; }

  public Credentials() : this(string.Empty, string.Empty)
  {
  }

  public Credentials(string locale, string emailAddress, string? password = null, bool usePasswordless = false)
  {
    Locale = locale;
    EmailAddress = emailAddress;
    Password = password;
    UsePasswordless = usePasswordless;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<Credentials>
  {
    public Validator()
    {
      RuleFor(x => x.Locale).Locale();
      RuleFor(x => x.EmailAddress).EmailAddressValue();
      When(x => x.Password is not null, () => RuleFor(x => x.Password).NotEmpty());
      When(x => x.UsePasswordless, () => RuleFor(x => x.Password).Null());
    }
  }
}
