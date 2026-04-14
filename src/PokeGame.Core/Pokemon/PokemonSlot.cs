using FluentValidation;

namespace PokeGame.Core.Pokemon;

public record PokemonSlot
{
  public const int BoxCount = 32;
  public const int BoxSize = 5 * 6;
  public const int PartySize = 6;

  public int Position { get; }
  public int? Box { get; }

  public PokemonSlot(int position, int? box = null)
  {
    Position = position;
    Box = box;
    new Validator().ValidateAndThrow(this);
  }

  private class Validator : AbstractValidator<PokemonSlot>
  {
    public Validator()
    {
      When(x => x.Box.HasValue, () => RuleFor(x => x.Position).InclusiveBetween(0, BoxSize - 1))
        .Otherwise(() => RuleFor(x => x.Position).InclusiveBetween(0, PartySize - 1));
      RuleFor(x => x.Box).InclusiveBetween(0, BoxCount - 1);
    }
  }
}
