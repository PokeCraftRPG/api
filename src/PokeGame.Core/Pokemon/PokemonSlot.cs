using FluentValidation;

namespace PokeGame.Core.Pokemon;

public record PokemonSlot
{
  public const int BoxCount = 32;
  public const int BoxSize = 5 * 6;
  public const int PartySize = PokemonParty.MaximumSize; // TODO(fpion): remove this const

  public int Position { get; }
  public int? Box { get; }

  public PokemonSlot(int position, int? box = null)
  {
    Position = position;
    Box = box;
    new Validator().ValidateAndThrow(this);
  }

  public bool IsGreaterThan(PokemonSlot slot)
  {
    if (Box != slot.Box)
    {
      throw new ArgumentException("Cannot compare slots that are not in the same box/party.", nameof(slot));
    }

    return Position > slot.Position;
  }

  public bool IsLessThan(PokemonSlot slot)
  {
    if (Box != slot.Box)
    {
      throw new ArgumentException("Cannot compare slots that are not in the same box/party.", nameof(slot));
    }

    return Position < slot.Position;
  }

  public PokemonSlot Next()
  {
    if (Box.HasValue)
    {
      if (Position == (BoxSize - 1))
      {
        if (Box == (BoxCount - 1))
        {
          throw new InvalidOperationException("The current slot is the last boxed slot.");
        }

        return new PokemonSlot(0, Box + 1);
      }

      return new PokemonSlot(Position + 1, Box);
    }
    else if (Position == (PartySize - 1))
    {
      throw new InvalidOperationException("The current slot is the last party slot.");
    }

    return new PokemonSlot(Position + 1);
  }

  public PokemonSlot Previous()
  {
    if (Box.HasValue)
    {
      if (Position == 0)
      {
        if (Box == 0)
        {
          throw new InvalidOperationException("The current slot is the first boxed slot.");
        }

        return new PokemonSlot(BoxSize - 1, Box - 1);
      }

      return new PokemonSlot(Position - 1, Box);
    }
    else if (Position == 0)
    {
      throw new InvalidOperationException("The current slot is the first party slot.");
    }

    return new PokemonSlot(Position - 1);
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
