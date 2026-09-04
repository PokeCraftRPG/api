using FluentValidation;

namespace PokeGame.Core.Species;

public interface ISpeciesEggs
{
  int Cycles { get; }
  EggGroup PrimaryGroup { get; }
  EggGroup? SecondaryGroup { get; }
}

public sealed record SpeciesEggs : ISpeciesEggs
{
  public const int MaximumCycles = byte.MaxValue;

  public int Cycles { get; }
  public EggGroup PrimaryGroup { get; }
  public EggGroup? SecondaryGroup { get; }

  public SpeciesEggs(int cycles = MaximumCycles, EggGroup primaryGroup = default, EggGroup? secondaryGroup = null)
  {
    Cycles = cycles;
    PrimaryGroup = primaryGroup;
    SecondaryGroup = secondaryGroup;
    new SpeciesEggsValidator().ValidateAndThrow(this);
  }

  public static SpeciesEggs From(ISpeciesEggs eggs) => new(eggs.Cycles, eggs.PrimaryGroup, eggs.SecondaryGroup);
}

internal class SpeciesEggsValidator : AbstractValidator<ISpeciesEggs>
{
  public SpeciesEggsValidator()
  {
    RuleFor(x => x.Cycles).InclusiveBetween(1, SpeciesEggs.MaximumCycles);
    RuleFor(x => x.PrimaryGroup).IsInEnum();
    When(x => x.PrimaryGroup == EggGroup.NoEggsDiscovered || x.PrimaryGroup == EggGroup.Ditto, () => RuleFor(x => x.SecondaryGroup).Null());
    RuleFor(x => x.SecondaryGroup).IsInEnum().NotEqual(x => x.PrimaryGroup);
  }
}
