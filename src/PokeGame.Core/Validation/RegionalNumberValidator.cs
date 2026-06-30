using FluentValidation;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Validation;

internal class RegionalNumberValidator : AbstractValidator<RegionalNumberPayload>
{
  public RegionalNumberValidator(bool allowZero = false)
  {
    RuleFor(x => x.Region).NotEmpty();
    RuleFor(x => x.Number).Number(allowZero);
  }
}
