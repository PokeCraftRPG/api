using FluentValidation;

namespace PokeGame.Core.Species.Models;

public record SetRegionalNumberPayload
{
  public int Number { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SetRegionalNumberPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Number).Number();
    }
  }
}
