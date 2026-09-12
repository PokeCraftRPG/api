using FluentValidation;
using PokeGame.Core.Abilities;

namespace PokeGame.Core.Pokemon.Models;

public record CreatePokemonPayload
{
  public Guid FormId { get; set; }

  public string? Key { get; set; }

  public Gender? Gender { get; set; }
  public bool? IsShiny { get; set; }
  public PokemonType? TeraType { get; set; }
  public AbilitySlot? AbilitySlot { get; set; }
  public byte? Size { get; set; }
  public string? Nature { get; set; }

  public byte EggCycles { get; set; }
  public int Experience { get; set; }

  public IndividualValuesDto? IndividualValues { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<CreatePokemonPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      RuleFor(x => x.Gender).IsInEnum();
      RuleFor(x => x.TeraType).IsInEnum();
      RuleFor(x => x.AbilitySlot).IsInEnum();
      When(x => !string.IsNullOrWhiteSpace(x.Nature), () => RuleFor(x => x.Nature!).Nature());

      RuleFor(x => x.Experience).GreaterThanOrEqualTo(0);
      RuleFor(x => x).Must(HaveAValidProgression)
        .WithErrorCode("PokemonProgressionValidator")
        .WithMessage("Egg cycles and experience cannot both be greater than zero.");

      When(x => x.IndividualValues is not null, () => RuleFor(x => x.IndividualValues!).SetValidator(new IndividualValuesValidator()));
    }

    private static bool HaveAValidProgression(CreatePokemonPayload payload)
    {
      if (payload.EggCycles > 0)
      {
        return payload.Experience < 1;
      }
      if (payload.Experience > 0)
      {
        return payload.EggCycles < 1;
      }
      return true;
    }
  }
}
