using FluentValidation;
using Krakenar.Contracts.Settings;

namespace PokeGame.Core.Validation;

public static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, byte> Accuracy<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.Accuracy.MinimumValue, Moves.Accuracy.MaximumValue);
  }

  public static IRuleBuilderOptions<T, byte> CatchRate<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.GreaterThan((byte)0);
  }

  public static IRuleBuilderOptions<T, DateTime> DateOfBirth<T>(this IRuleBuilder<T, DateTime> ruleBuilder, DateTime? moment = null, int minimumAge = 18, int maximumAge = 100)
  {
    moment ??= DateTime.Now;
    return ruleBuilder.InclusiveBetween(moment.Value.AddYears(-maximumAge), moment.Value.AddYears(-minimumAge));
  }

  public static IRuleBuilderOptions<T, string> Description<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, byte> EggCycles<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.GreaterThan((byte)0);
  }

  public static IRuleBuilderOptions<T, string> EmailAddressValue<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(byte.MaxValue).EmailAddress();
  }

  public static IRuleBuilderOptions<T, DateTime> Future<T>(this IRuleBuilder<T, DateTime> ruleBuilder, DateTime? moment = null)
  {
    return ruleBuilder.SetValidator(new FutureValidator<T>(moment));
  }

  public static IRuleBuilderOptions<T, string> Gender<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(10).SetValidator(new GenderValidator<T>());
  }

  public static IRuleBuilderOptions<T, int> GenderRatio<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Varieties.GenderRatio.MinimumValue, Varieties.GenderRatio.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> Genus<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Varieties.Genus.MaximumLength);
  }

  public static IRuleBuilderOptions<T, int> Height<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.GreaterThan(0);
  }

  public static IRuleBuilderOptions<T, int> Level<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Pokemon.Level.MinimumValue, Pokemon.Level.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> License<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Trainers.License.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Locale<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(16).SetValidator(new LocaleValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Location<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Regions.Location.MaximumLength);
  }

  public static IRuleBuilderOptions<T, int> Money<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.GreaterThanOrEqualTo(0);
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Name.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Nature<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.SetValidator(new PokemonNatureValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Notes<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, int> Number<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Species.Number.MinimumValue, Species.Number.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder, IPasswordSettings settings)
  {
    IRuleBuilderOptions<T, string> options = ruleBuilder.NotEmpty();
    if (settings.RequiredLength > 0)
    {
      options = options.MinimumLength(settings.RequiredLength)
        .WithErrorCode("PasswordTooShort")
        .WithMessage($"Passwords must be at least {settings.RequiredLength} characters.");
    }
    if (settings.RequiredUniqueChars > 0)
    {
      options = options.Must(x => x.GroupBy(c => c).Count() >= settings.RequiredUniqueChars)
        .WithErrorCode("PasswordRequiresUniqueChars")
        .WithMessage($"Passwords must use at least {settings.RequiredUniqueChars} different characters.");
    }
    if (settings.RequireNonAlphanumeric)
    {
      options = options.Must(x => x.Any(c => !char.IsLetterOrDigit(c)))
        .WithErrorCode("PasswordRequiresNonAlphanumeric")
        .WithMessage("Passwords must have at least one non alphanumeric character.");
    }
    if (settings.RequireLowercase)
    {
      options = options.Must(x => x.Any(char.IsLower))
        .WithErrorCode("PasswordRequiresLower")
        .WithMessage("Passwords must have at least one lowercase ('a'-'z').");
    }
    if (settings.RequireUppercase)
    {
      options = options.Must(x => x.Any(char.IsUpper))
        .WithErrorCode("PasswordRequiresUpper")
        .WithMessage("Passwords must have at least one uppercase ('A'-'Z').");
    }
    if (settings.RequireDigit)
    {
      options = options.Must(x => x.Any(char.IsDigit))
        .WithErrorCode("PasswordRequiresDigit")
        .WithMessage("Passwords must have at least one digit ('0'-'9').");
    }
    return options;
  }

  public static IRuleBuilderOptions<T, DateTime> Past<T>(this IRuleBuilder<T, DateTime> ruleBuilder, DateTime? moment = null)
  {
    return ruleBuilder.SetValidator(new PastValidator<T>(moment));
  }

  public static IRuleBuilderOptions<T, byte> Power<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.Power.MinimumValue, Moves.Power.MaximumValue);
  }

  public static IRuleBuilderOptions<T, byte> PowerPoints<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.PowerPoints.MinimumValue, Moves.PowerPoints.MaximumValue);
  }

  public static IRuleBuilderOptions<T, int> Price<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.GreaterThan(0);
  }

  public static IRuleBuilderOptions<T, string> Slug<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Slug.MaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> TimeZone<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(32).SetValidator(new TimeZoneValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Url.MaximumLength).SetValidator(new UrlValidator<T>());
  }

  public static IRuleBuilderOptions<T, int> Weight<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.GreaterThan(0);
  }
}
