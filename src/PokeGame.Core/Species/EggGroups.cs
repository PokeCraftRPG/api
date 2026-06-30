using FluentValidation;

namespace PokeGame.Core.Species;

public interface IEggGroups
{
  EggGroup Primary { get; }
  EggGroup? Secondary { get; }
}

public class EggGroupsValidator : AbstractValidator<IEggGroups>
{
  public EggGroupsValidator()
  {
    RuleFor(x => x.Primary).IsInEnum();
    When(x => x.Secondary.HasValue, () => RuleFor(x => x.Secondary!.Value).IsInEnum().NotEqual(x => x.Primary));
    When(x => x.Primary == EggGroup.NoEggsDiscovered || x.Primary == EggGroup.Ditto, () => RuleFor(x => x.Secondary).Null());
    When(x => x.Secondary.HasValue, () => RuleFor(x => x.Secondary!.Value)
      .NotEqual(EggGroup.NoEggsDiscovered)
      .NotEqual(EggGroup.Ditto));
  }
}
