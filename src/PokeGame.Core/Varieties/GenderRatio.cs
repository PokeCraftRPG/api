using FluentValidation;

namespace PokeGame.Core.Varieties;

public sealed class GenderRatio
{
  public static GenderRatio AllFemale => new(8);
  public static GenderRatio AllMale => new(0);

  public int FemaleRate { get; }
  public int MaleRate => 8 - FemaleRate;

  public GenderRatio(int femaleRate)
  {
    FemaleRate = femaleRate;
    new Validator().ValidateAndThrow(this);
  }

  public static GenderRatio? TryCreate(int? femaleRate) => femaleRate.HasValue ? new(femaleRate.Value) : null;

  public override bool Equals(object? obj) => obj is GenderRatio genderRatio && genderRatio.FemaleRate == FemaleRate;
  public override int GetHashCode() => FemaleRate.GetHashCode();
  public override string ToString() => FemaleRate.ToString();

  private class Validator : AbstractValidator<GenderRatio>
  {
    public Validator()
    {
      RuleFor(x => x.FemaleRate).GenderRatio();
    }
  }
}
