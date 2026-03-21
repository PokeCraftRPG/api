using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Worlds.Models;

public record UpdateWorldPayload
{
  public string? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateWorldPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());
    }
  }
}
