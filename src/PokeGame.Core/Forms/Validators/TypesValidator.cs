using FluentValidation;

namespace PokeGame.Core.Forms.Validators;

internal class TypesValidator : AbstractValidator<IFormTypes>
{
  public TypesValidator()
  {
    RuleFor(x => x.Primary).IsInEnum();
    RuleFor(x => x.Secondary).IsInEnum().NotEqual(x => x.Primary);
  }
}
