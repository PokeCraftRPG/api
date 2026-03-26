using FluentValidation;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms.Validators;

internal class AbilitiesValidator : AbstractValidator<AbilitiesPayload>
{
  public AbilitiesValidator()
  {
    RuleFor(x => x.Primary).NotEmpty();
  }
}
