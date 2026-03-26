using FluentValidation;

namespace PokeGame.Core.Forms.Validators;

internal class YieldValidator : AbstractValidator<IYield>
{
  private const int MinimumTotal = 1;
  private const int MaximumTotal = 4;
  private const int MinimumValue = 0;
  private const int MaximumValue = 3;

  public YieldValidator()
  {
    RuleFor(x => x.Experience).GreaterThan(0);

    RuleFor(x => x.HP).InclusiveBetween(MinimumValue, MaximumValue);
    RuleFor(x => x.Attack).InclusiveBetween(MinimumValue, MaximumValue);
    RuleFor(x => x.Defense).InclusiveBetween(MinimumValue, MaximumValue);
    RuleFor(x => x.SpecialAttack).InclusiveBetween(MinimumValue, MaximumValue);
    RuleFor(x => x.SpecialDefense).InclusiveBetween(MinimumValue, MaximumValue);
    RuleFor(x => x.Speed).InclusiveBetween(MinimumValue, MaximumValue);

    RuleFor(x => x).Must(BeAValidYield)
      .WithErrorCode(nameof(YieldValidator))
      .WithMessage($"The total Effort Value (EV) yield should vary from {MinimumTotal} to {MaximumTotal}.");
  }

  private static bool BeAValidYield(IYield yield)
  {
    int total = yield.HP + yield.Attack + yield.Defense + yield.SpecialAttack + yield.SpecialDefense + yield.Speed;
    return total >= MinimumTotal && total <= MaximumTotal;
  }
}
