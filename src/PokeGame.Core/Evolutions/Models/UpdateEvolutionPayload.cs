using FluentValidation;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Evolutions.Models;

public record UpdateEvolutionPayload
{
  public Optional<int?>? Level { get; set; }
  public bool? Friendship { get; set; }
  public Optional<PokemonGender?>? Gender { get; set; }
  public Optional<string>? HeldItem { get; set; }
  public Optional<string>? KnownMove { get; set; }
  public Optional<string>? Location { get; set; }
  public Optional<TimeOfDay?>? TimeOfDay { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateEvolutionPayload>
  {
    public Validator()
    {
      When(x => x.Level?.Value is not null, () => RuleFor(x => x.Level!.Value!.Value).Level());
      When(x => x.Gender is not null, () => RuleFor(x => x.Gender!.Value).IsInEnum());
      When(x => !string.IsNullOrWhiteSpace(x.Location?.Value), () => RuleFor(x => x.Location!.Value!).Location());
      When(x => x.TimeOfDay is not null, () => RuleFor(x => x.TimeOfDay!.Value).IsInEnum());
    }
  }
}
