using FluentValidation;
using PokeGame.Core.Abilities;
using PokeGame.Core.Pokemon.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon.Models;

public record CreatePokemonPayload
{
  public Guid? Id { get; set; }

  public string Form { get; set; }

  public string? Key { get; set; }
  public string? Name { get; set; }
  public PokemonGender? Gender { get; set; }
  public bool IsShiny { get; set; }

  public PokemonType? TeraType { get; set; }
  public PokemonSizePayload? Size { get; set; }
  public AbilitySlot? AbilitySlot { get; set; }
  public string? Nature { get; set; }

  public byte? EggCycles { get; set; }
  public int Experience { get; set; }

  public IndividualValuesModel? IndividualValues { get; set; }
  public EffortValuesModel? EffortValues { get; set; }
  public int? Vitality { get; set; }
  public int? Stamina { get; set; }
  public byte? Friendship { get; set; }

  public string? HeldItem { get; set; }

  public string? Sprite { get; set; }
  public string? Url { get; set; }
  public string? Notes { get; set; }

  public CreatePokemonPayload() : this(string.Empty)
  {
  }

  public CreatePokemonPayload(string form)
  {
    Form = form;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreatePokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Form).NotEmpty();

      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      RuleFor(x => x.Gender).IsInEnum();

      RuleFor(x => x.TeraType).IsInEnum();
      RuleFor(x => x.AbilitySlot).IsInEnum();
      When(x => !string.IsNullOrWhiteSpace(x.Nature), () => RuleFor(x => x.Nature!).Nature());

      When(x => x.EggCycles.HasValue, () =>
      {
        RuleFor(x => x.EggCycles!.Value).EggCycles();
        RuleFor(x => x.Experience).Equal(0);
      });
      RuleFor(x => x.Experience).GreaterThanOrEqualTo(0);
      When(x => x.Experience > 0, () => RuleFor(x => x.EggCycles).Null());

      When(x => x.IndividualValues is not null, () => RuleFor(x => x.IndividualValues!).SetValidator(new IndividualValuesValidator()));
      When(x => x.EffortValues is not null, () => RuleFor(x => x.EffortValues!).SetValidator(new EffortValuesValidator()));
      When(x => x.Vitality.HasValue, () => RuleFor(x => x.Vitality!.Value).Vitality());
      When(x => x.Stamina.HasValue, () => RuleFor(x => x.Stamina!.Value).Stamina());

      When(x => !string.IsNullOrWhiteSpace(x.Sprite), () => RuleFor(x => x.Sprite!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());
    }
  }
}
