using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record CatchPokemonPayload
{
  public Guid TrainerId { get; set; }
  public Guid PokeBallId { get; set; }

  public string Location { get; set; } = string.Empty;

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<CatchPokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Location).Location();
    }
  }
}
