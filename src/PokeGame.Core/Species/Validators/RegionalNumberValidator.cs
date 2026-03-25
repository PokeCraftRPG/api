using FluentValidation;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Validators;

internal class RegionalNumberValidator : AbstractValidator<RegionalNumberPayload>
{
  public RegionalNumberValidator(int minimum = Number.MinimumValue)
  {
    RuleFor(x => x.Number).InclusiveBetween(minimum, Number.MaximumValue);
  }
}
