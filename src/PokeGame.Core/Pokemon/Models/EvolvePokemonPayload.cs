using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record EvolvePokemonPayload
{
  public Guid EvolutionId { get; set; }

  public string? Location { get; set; }
  public TimeOfDay? TimeOfDay { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<EvolvePokemonPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Location), () => RuleFor(x => x.Location!).Location());
      RuleFor(x => x.TimeOfDay).IsInEnum();
    }
  }
}
