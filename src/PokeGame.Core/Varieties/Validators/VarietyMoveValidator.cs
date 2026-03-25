using FluentValidation;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Validators;

internal class VarietyMoveValidator : AbstractValidator<VarietyMovePayload>
{
  public VarietyMoveValidator(bool allowNullLevel = false)
  {
    RuleFor(x => x.Move).NotEmpty();

    if (!allowNullLevel)
    {
      RuleFor(x => x.Level).NotNull();
    }
    When(x => x.Level.HasValue, () => RuleFor(x => x.Level!.Value).InclusiveBetween(0, Level.MaximumValue));
  }
}
