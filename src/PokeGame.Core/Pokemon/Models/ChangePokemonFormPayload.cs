using FluentValidation;

namespace PokeGame.Core.Pokemon.Models;

public record ChangePokemonFormPayload
{
  public string Form { get; set; }

  public ChangePokemonFormPayload() : this(string.Empty)
  {
  }

  public ChangePokemonFormPayload(string form)
  {
    Form = form;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<ChangePokemonFormPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Form).NotEmpty();
    }
  }
}
