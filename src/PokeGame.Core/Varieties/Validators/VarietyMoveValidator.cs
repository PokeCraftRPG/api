using FluentValidation;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Validators;

internal class VarietyMoveValidator : AbstractValidator<VarietyMovePayload>
{
  public VarietyMoveValidator()
  {
    RuleFor(x => x.Level).Level(); // TODO(fpion): should 0 be allowed? should null be allowed? How to differenciate between removing, evolution and level moves?
  }
}
