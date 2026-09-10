using FluentValidation;

namespace PokeGame.Core.Pokemon;

public interface IPokemonNature
{
  string Name { get; }
  PokemonStatistic? IncreasedStatistic { get; }
  PokemonStatistic? DecreasedStatistic { get; }
  Flavor? FavoriteFlavor { get; }
  Flavor? DislikedFlavor { get; }
}

public sealed record PokemonNature : IPokemonNature
{
  public const int MaximumLength = 8;

  public string Name { get; }
  public PokemonStatistic? IncreasedStatistic { get; }
  public PokemonStatistic? DecreasedStatistic { get; }
  public Flavor? FavoriteFlavor { get; }
  public Flavor? DislikedFlavor { get; }

  public PokemonNature(
    string name,
    PokemonStatistic? increasedStatistic = null,
    PokemonStatistic? decreasedStatistic = null,
    Flavor? favoriteFlavor = null,
    Flavor? dislikedFlavor = null)
  {
    Name = name.Trim();
    IncreasedStatistic = increasedStatistic;
    DecreasedStatistic = decreasedStatistic;
    FavoriteFlavor = favoriteFlavor;
    DislikedFlavor = dislikedFlavor;
    new PokemonNatureValidator().ValidateAndThrow(this);
  }

  public PokemonNature(IPokemonNature nature) : this(nature.Name, nature.IncreasedStatistic, nature.DecreasedStatistic, nature.FavoriteFlavor, nature.DislikedFlavor)
  {
  }

  public double GetMultiplier(PokemonStatistic statistic)
  {
    if (statistic == IncreasedStatistic)
    {
      return 1.1;
    }
    else if (statistic == DecreasedStatistic)
    {
      return 0.9;
    }
    return 1.0;
  }

  public override string ToString() => Name;
}

internal class PokemonNatureValidator : AbstractValidator<IPokemonNature>
{
  public PokemonNatureValidator()
  {
    RuleFor(x => x.Name).NotEmpty().MaximumLength(PokemonNature.MaximumLength);
    RuleFor(x => x.IncreasedStatistic).IsInEnum();
    RuleFor(x => x.DecreasedStatistic).IsInEnum();
    RuleFor(x => x.FavoriteFlavor).IsInEnum();
    RuleFor(x => x.DislikedFlavor).IsInEnum();

    RuleFor(x => x).Must(HaveValidStatistics)
      .WithErrorCode("NatureStatisticsValidator")
      .WithMessage("The increased and decreased statistics must either both be specified and different, or both be omitted.");
    RuleFor(x => x).Must(HaveValidFlavors)
      .WithErrorCode("NatureFlavorsValidator")
      .WithMessage("The favorite and disliked flavors must either both be specified and different, or both be omitted.");
  }

  private static bool HaveValidStatistics(IPokemonNature nature) => !nature.IncreasedStatistic.HasValue || !nature.DecreasedStatistic.HasValue
    ? !nature.IncreasedStatistic.HasValue && !nature.DecreasedStatistic.HasValue
    : nature.IncreasedStatistic.Value != nature.DecreasedStatistic.Value;

  private static bool HaveValidFlavors(IPokemonNature nature) => !nature.FavoriteFlavor.HasValue || !nature.DislikedFlavor.HasValue
    ? !nature.FavoriteFlavor.HasValue && !nature.DislikedFlavor.HasValue
    : nature.FavoriteFlavor.Value != nature.DislikedFlavor.Value;
}
