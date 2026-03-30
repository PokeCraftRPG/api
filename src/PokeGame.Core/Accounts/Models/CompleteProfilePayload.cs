using FluentValidation;
using PokeGame.Core.Identity;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Accounts.Models;

public record CompleteProfilePayload
{
  public string Token { get; set; }

  public string? Password { get; set; }
  public MultiFactorAuthenticationMode MultiFactorAuthenticationMode { get; set; }

  public string FirstName { get; set; }
  public string LastName { get; set; }

  public DateTime? DateOfBirth { get; set; }
  public string? Gender { get; set; }
  public string TimeZone { get; set; }

  public CompleteProfilePayload() : this(string.Empty, string.Empty, string.Empty, string.Empty)
  {
  }

  public CompleteProfilePayload(string token, string firstName, string lastName, string timeZone)
  {
    Token = token;
    FirstName = firstName;
    LastName = lastName;
    TimeZone = timeZone;
  }
}

internal class CompleteProfilePayloadValidator : AbstractValidator<CompleteProfilePayload>
{
  public CompleteProfilePayloadValidator()
  {
    RuleFor(x => x.Token).NotEmpty();

    // TODO(fpion): When(x => x.Password is not null, () => RuleFor(x => x.Password!).Password(passwordSettings));
    RuleFor(x => x.MultiFactorAuthenticationMode).IsInEnum();
    When(x => x.MultiFactorAuthenticationMode != MultiFactorAuthenticationMode.None, () => RuleFor(x => x.Password).NotNull());

    RuleFor(x => x.FirstName).Name();
    RuleFor(x => x.LastName).Name();

    When(x => x.DateOfBirth.HasValue, () => RuleFor(x => x.DateOfBirth!.Value).DateOfBirth());
    When(x => !string.IsNullOrWhiteSpace(x.Gender), () => RuleFor(x => x.Gender!).Gender());
    RuleFor(x => x.TimeZone).TimeZone();
  }
}
