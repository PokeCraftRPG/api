using FluentValidation;
using PokeGame.Core.Battles.Validators;
using PokeGame.Core.Items.Properties;

namespace PokeGame.Core.Items.Validators;

internal class BattleItemValidator : AbstractValidator<IBattleItemProperties>
{
  public BattleItemValidator()
  {
    RuleFor(x => x).SetValidator(new StatisticChangesValidator());
    RuleFor(x => x.GuardTurns).InclusiveBetween(0, 10);
  }
}
