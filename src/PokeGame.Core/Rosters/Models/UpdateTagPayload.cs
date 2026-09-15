using FluentValidation;

namespace PokeGame.Core.Rosters.Models;

public record UpdateTagPayload
{
  public string? Name { get; set; }
  public Optional<ColorDto>? Color { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<UpdateTagPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
    }
  }
}
