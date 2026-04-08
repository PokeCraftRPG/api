using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon.Models;

public record UpdatePokemonPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }

  public Optional<string>? Sprite { get; set; }
  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  // TODO(fpion): Form, Gender, IsShiny, TeraType, Size, AbilitySlot, Nature, EggCycles, Experience, IVs, EVs, Vitality, Stamina, StatusCondition, Friendship, HeldItem

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdatePokemonPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());

      When(x => !string.IsNullOrWhiteSpace(x.Sprite?.Value), () => RuleFor(x => x.Sprite!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());
    }
  }
}
