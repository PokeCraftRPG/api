using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record ReceivePokemonPayload
{
  public Guid TrainerId { get; set; }
  public Guid PokeBallId { get; set; }

  public string Location { get; set; } = string.Empty;

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<ReceivePokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Location).Location();
    }
  }
}
