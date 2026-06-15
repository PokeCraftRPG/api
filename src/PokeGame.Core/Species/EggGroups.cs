using FluentValidation;

namespace PokeGame.Core.Species;

public interface IEggGroups
{
  EggGroup Primary { get; }
  EggGroup? Secondary { get; }
}

public record EggGroups : IEggGroups
{
  public EggGroup Primary { get; }
  public EggGroup? Secondary { get; }

  public EggGroups()
  {
  }

  [JsonConstructor]
  public EggGroups(EggGroup primary, EggGroup? secondary = null)
  {
    Primary = primary;
    Secondary = secondary;
    new EggGroupsValidator().ValidateAndThrow(this);
  }

  public EggGroups(IEggGroups groups) : this(groups.Primary, groups.Secondary)
  {
  }
}

public class EggGroupsValidator : AbstractValidator<IEggGroups>
{
  public EggGroupsValidator()
  {
    RuleFor(x => x.Primary).IsInEnum();
    When(x => x.Secondary.HasValue, () => RuleFor(x => x.Secondary!.Value).IsInEnum().NotEqual(x => x.Primary));
    When(x => x.Primary == EggGroup.NoEggsDiscovered || x.Primary == EggGroup.Ditto, () => RuleFor(x => x.Secondary).Null());
  }
}
