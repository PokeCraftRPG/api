using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record UpdatePokemonPayload
{
  public string? Key { get; set; }

  public Optional<string>? Nickname { get; set; }

  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  // TODO(fpion): Gender
  // TODO(fpion): IsShiny
  // TODO(fpion): TeraType
  // TODO(fpion): AbilitySlot
  // TODO(fpion): Size
  // TODO(fpion): Nature

  // TODO(fpion): EggCycles
  // TODO(fpion): Experience

  // TODO(fpion): SkillRanks

  // TODO(fpion): IndividualValues

  // TODO(fpion): Vitality
  // TODO(fpion): Stamina
  // TODO(fpion): Condition
  // TODO(fpion): Friendship

  // TODO(fpion): Characteristic

  public Optional<Guid?>? HeldItemId { get; set; }

  public Optional<Guid?>? SpriteId { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdatePokemonPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Nickname?.Value), () => RuleFor(x => x.Nickname!.Value!).Name());

      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());
    }
  }
}
