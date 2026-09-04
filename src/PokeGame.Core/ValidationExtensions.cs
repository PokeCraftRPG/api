using FluentValidation;
using Krakenar.Contracts.Settings;
using PokeGame.Core.Identity;
using PokeGame.Core.Seo;

namespace PokeGame.Core;

internal static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, int> Accuracy<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 100);
  }

  public static IRuleBuilderOptions<T, int> CatchRate<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, Species.CatchRate.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> Content<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, DateTime> DateOfBirth<T>(this IRuleBuilder<T, DateTime> ruleBuilder, DateTime? moment = null, int minimumAge = 18, int maximumAge = 100)
  {
    moment ??= DateTime.Now;
    return ruleBuilder.InclusiveBetween(moment.Value.AddYears(-maximumAge), moment.Value.AddYears(-minimumAge));
  }

  public static IRuleBuilderOptions<T, string> EmailAddressValue<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(byte.MaxValue).EmailAddress();
  }

  public static IRuleBuilderOptions<T, int> Friendship<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(0, byte.MaxValue);
  }

  public static IRuleBuilderOptions<T, string> Gender<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(10).SetValidator(new GenderValidator<T>());
  }

  public static IRuleBuilderOptions<T, int> GenderRatio<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(0, 8);
  }

  public static IRuleBuilderOptions<T, string> Genus<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Varieties.Genus.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Key<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Key.MaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Locale<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(LocaleValidator<T>.MaximumLength).SetValidator(new LocaleValidator<T>());
  }

  public static IRuleBuilderOptions<T, IEnumerable<TCollection>> MaximumCount<T, TCollection>(this IRuleBuilder<T, IEnumerable<TCollection>> ruleBuilder, int maximumCount = 10)
  {
    return ruleBuilder.Must(collection => collection.Count() <= maximumCount)
      .WithErrorCode("MaximumCountValidator")
      .WithMessage($"'{{PropertyName}}' may only include up to {maximumCount} items.");
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Name.MaximumLength);
  }

  public static IRuleBuilderOptions<T, int> Number<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 9999);
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

  public static IRuleBuilderOptions<T, string> PersonName<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(byte.MaxValue);
  }

  public static IRuleBuilderOptions<T, int> Power<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 250);
  }

  public static IRuleBuilderOptions<T, int> PowerPoints<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 40);
  }

  public static IRuleBuilderOptions<T, string> Summary<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Summary.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> TimeZone<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(32).SetValidator(new TimeZoneValidator<T>());
  }
}
