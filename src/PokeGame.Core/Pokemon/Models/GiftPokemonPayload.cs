using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon.Models;

public record GiftPokemonPayload
{
  public string Trainer { get; set; }

  public string Location { get; set; }

  public GiftPokemonPayload() : this(string.Empty, string.Empty)
  {
  }

  public GiftPokemonPayload(string trainer, string location)
  {
    Trainer = trainer;
    Location = location;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<GiftPokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Trainer).NotEmpty();

      RuleFor(x => x.Location).Location();
    }
  }
}
