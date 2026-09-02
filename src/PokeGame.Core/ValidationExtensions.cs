using FluentValidation;
using PokeGame.Core.Seo;

namespace PokeGame.Core;

internal static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, string> Content<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty();
  }

  public static IRuleBuilderOptions<T, string> Key<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Key.MaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Name.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Summary<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Summary.MaximumLength);
  }
}
