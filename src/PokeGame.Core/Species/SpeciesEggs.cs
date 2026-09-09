using FluentValidation;

namespace PokeGame.Core.Species;

public interface ISpeciesEggs
{
  byte Cycles { get; }
  EggGroup PrimaryGroup { get; }
  EggGroup? SecondaryGroup { get; }
}

public sealed record SpeciesEggs : ISpeciesEggs
{
  public byte Cycles { get; }
  public EggGroup PrimaryGroup { get; }
  public EggGroup? SecondaryGroup { get; }

  public SpeciesEggs(byte cycles, EggGroup primaryGroup = EggGroup.NoEggsDiscovered, EggGroup? secondaryGroup = null)
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
    RuleFor(x => x.Cycles).GreaterThan((byte)0);
    RuleFor(x => x.PrimaryGroup).IsInEnum();
    When(x => x.PrimaryGroup == EggGroup.NoEggsDiscovered || x.PrimaryGroup == EggGroup.Ditto, () => RuleFor(x => x.SecondaryGroup).Null());
    RuleFor(x => x.SecondaryGroup).IsInEnum().NotEqual(x => x.PrimaryGroup);
  }
}
