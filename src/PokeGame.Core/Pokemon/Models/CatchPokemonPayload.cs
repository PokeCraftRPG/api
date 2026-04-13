using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon.Models;

public record CatchPokemonPayload
{
  public string Trainer { get; set; }
  public string PokeBall { get; set; }

  public string Location { get; set; }

  public CatchPokemonPayload() : this(string.Empty, string.Empty, string.Empty)
  {
  }

  public CatchPokemonPayload(string trainer, string pokeBall, string location)
  {
    Trainer = trainer;
    PokeBall = pokeBall;
    Location = location;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CatchPokemonPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Trainer).NotEmpty();
      RuleFor(x => x.PokeBall).NotEmpty();

      RuleFor(x => x.Location).Location();
    }
  }
}
