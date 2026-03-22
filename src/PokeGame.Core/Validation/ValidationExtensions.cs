using FluentValidation;

namespace PokeGame.Core.Validation;

public static class ValidationExtensions
{
  public static IRuleBuilderOptions<T, byte> Accuracy<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.Accuracy.MinimumValue, Moves.Accuracy.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> Description<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Description.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Name<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Name.MaximumLength);
  }

  public static IRuleBuilderOptions<T, string> Notes<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Notes.MaximumLength);
  }

  public static IRuleBuilderOptions<T, byte> Power<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.Power.MinimumValue, Moves.Power.MaximumValue);
  }

  public static IRuleBuilderOptions<T, byte> PowerPoints<T>(this IRuleBuilder<T, byte> ruleBuilder)
  {
    return ruleBuilder.InclusiveBetween(Moves.PowerPoints.MinimumValue, Moves.PowerPoints.MaximumValue);
  }

  public static IRuleBuilderOptions<T, string> Slug<T>(this IRuleBuilder<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Slug.MaximumLength).SetValidator(new SlugValidator<T>());
  }

  public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
  {
    return ruleBuilder.NotEmpty().MaximumLength(Core.Url.MaximumLength).SetValidator(new UrlValidator<T>());
  }
}
