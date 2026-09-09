using FluentValidation;

namespace PokeGame.Core.Varieties;

public sealed class GenderRatio
{
  public const byte FemaleRatio = 8;
  public const byte MaleRatio = 0;

  public static GenderRatio AllFemale => new(FemaleRatio);
  public static GenderRatio AllMale => new(MaleRatio);

  public byte FemaleRate { get; }
  public byte MaleRate => (byte)(FemaleRatio - FemaleRate);

  public GenderRatio(byte femaleRate)
  {
    FemaleRate = femaleRate;
    new Validator().ValidateAndThrow(this);
  }

  public static GenderRatio? TryCreate(byte? femaleRate) => femaleRate.HasValue ? new(femaleRate.Value) : null;

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
