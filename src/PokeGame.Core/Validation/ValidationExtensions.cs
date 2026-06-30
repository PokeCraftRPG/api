using FluentValidation;

namespace PokeGame.Core.Validation;

internal static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, byte> Accuracy<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween((byte)1, (byte)100);
  }

  public static IRuleBuilderOptions<T, string> Description<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, string> Key<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Constants.KeyMaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Constants.NameMaximumLength);
  }

  public static IRuleBuilderOptions<T, byte> Power<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween((byte)1, (byte)250);
  }

  public static IRuleBuilderOptions<T, byte> PowerPoints<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween((byte)1, (byte)40);
  }
}
