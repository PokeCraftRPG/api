using FluentValidation;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Varieties;

public record GenderRatio
{
  public const byte MinimumValue = 0;
  public const byte MaximumValue = 8;

  public static GenderRatio AllFemale { get; } = new(MinimumValue);
  public static GenderRatio AllMale { get; } = new(MaximumValue);

  public byte Value { get; }

  public GenderRatio(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public bool IsValid(PokemonGender gender) => Value switch
  {
    MinimumValue => gender == PokemonGender.Female,
    MaximumValue => gender == PokemonGender.Male,
    _ => true,
  };

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<GenderRatio>
  {
    public Validator()
    {
      RuleFor(x => x.Value).GenderRatio();
    }
  }
}
