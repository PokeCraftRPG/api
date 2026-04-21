using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record MovePokemonPayload
{
  public int Position { get; set; }
  public int Box { get; set; }

  public MovePokemonPayload()
  {
  }

  public MovePokemonPayload(int position, int box)
  {
    Position = position;
    Box = box;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<MovePokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Position).InclusiveBetween(0, PokemonSlot.BoxSize - 1);
      RuleFor(x => x.Box).InclusiveBetween(0, PokemonSlot.BoxCount - 1);
    }
  }
}
