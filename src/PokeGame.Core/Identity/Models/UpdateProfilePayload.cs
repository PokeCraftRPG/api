using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Identity.Models;

public record UpdateProfilePayload
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }

  public Optional<DateTime?>? DateOfBirth { get; set; }
  public Optional<string>? Gender { get; set; }
  public string? Locale { get; set; }
  public string? TimeZone { get; set; }

  public UserExperience? DefaultExperience { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateProfilePayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.FirstName), () => RuleFor(x => x.FirstName!).PersonName());
      When(x => !string.IsNullOrWhiteSpace(x.LastName), () => RuleFor(x => x.LastName!).PersonName());

      When(x => x.DateOfBirth?.Value is not null, () => RuleFor(x => x.DateOfBirth!.Value!.Value).DateOfBirth());
      When(x => !string.IsNullOrWhiteSpace(x.Gender?.Value), () => RuleFor(x => x.Gender!.Value!).Gender());
      When(x => !string.IsNullOrWhiteSpace(x.Locale), () => RuleFor(x => x.Locale!).Locale());
      When(x => !string.IsNullOrWhiteSpace(x.TimeZone), () => RuleFor(x => x.TimeZone!).TimeZone());

      When(x => x.DefaultExperience.HasValue, () => RuleFor(x => x.DefaultExperience!.Value).IsInEnum());
    }
  }
}
