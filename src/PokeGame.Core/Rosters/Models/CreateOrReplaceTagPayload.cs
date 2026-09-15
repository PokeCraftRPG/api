using FluentValidation;

namespace PokeGame.Core.Rosters.Models;

public record CreateOrReplaceTagPayload
{
  public string Name { get; set; } = string.Empty;
  public ColorDto? Color { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceTagPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Name).Name();
    }
  }
}
