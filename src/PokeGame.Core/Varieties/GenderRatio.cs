using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Varieties;

public record GenderRatio
{
  public const int MinimumValue = 0;
  public const int MaximumValue = 8;

  public static GenderRatio AllFemale { get; } = new(MinimumValue);
  public static GenderRatio AllMale { get; } = new(MaximumValue);

  public int Value { get; }

  public GenderRatio(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<GenderRatio>
  {
    public Validator()
    {
      RuleFor(x => x.Value).GenderRatio();
    }
  }
}
