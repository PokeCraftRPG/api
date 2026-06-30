using FluentValidation;

namespace PokeGame.Core.Validation;

internal static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, int> Accuracy<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 100);
  }

  public static IRuleBuilderOptions<T, int> CatchRate<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 255);
  }

  public static IRuleBuilderOptions<T, string> Description<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, int> EggCycles<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 255);
  }

  public static IRuleBuilderOptions<T, int> Friendship<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(0, 255);
  }

  public static IRuleBuilderOptions<T, string> Key<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Constants.KeyMaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Constants.NameMaximumLength);
  }

  public static IRuleBuilderOptions<T, int> Number<T>(this IRuleBuilder<T, int> ruleBuilder, bool allowZero = false)
  {
    return ruleBuilder.InclusiveBetween(allowZero ? 0 : 1, 9999);
  }

  public static IRuleBuilderOptions<T, int> Power<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 250);
  }

  public static IRuleBuilderOptions<T, int> PowerPoints<T>(this IRuleBuilder<T, int> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(1, 40);
  }
}
