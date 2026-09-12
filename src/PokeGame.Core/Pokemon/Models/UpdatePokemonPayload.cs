using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record UpdatePokemonPayload
{
  // TODO(fpion): species, variety and form can be changed through Evolution.

  public string? Key { get; set; }

  public Optional<string>? Nickname { get; set; }

  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  // TODO(fpion): Gender, IsShiny, Size and Characteristic should never change.
  // TODO(fpion): TeraType (shards), AbilitySlot (patch/capsule) and Nature (mints) can change via complex processes.

  // TODO(fpion): EggCycles can be decreased (how?).
  // TODO(fpion): Experience can only be gained.

  // TODO(fpion): SkillRanks should have their own dedicated endpoint.

  // TODO(fpion): IndividualValues can never change. Hyper Training acts as an override and does not replace the actual IVs.

  public int? Vitality { get; set; }
  public int? Stamina { get; set; }
  public Optional<StatusCondition?>? Condition { get; set; }
  public byte? Friendship { get; set; }

  public Optional<Guid?>? HeldItemId { get; set; }

  public Optional<Guid?>? SpriteId { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<UpdatePokemonPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Nickname?.Value), () => RuleFor(x => x.Nickname!.Value!).Name());

      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      RuleFor(x => x.Vitality).GreaterThanOrEqualTo(0);
      RuleFor(x => x.Stamina).GreaterThanOrEqualTo(0);
      When(x => x.Condition is not null, () => RuleFor(x => x.Condition!.Value).IsInEnum());
    }
  }
}
