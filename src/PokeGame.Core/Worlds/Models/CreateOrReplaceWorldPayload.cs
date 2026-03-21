using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Worlds.Models;

public record CreateOrReplaceWorldPayload
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceWorldPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Name).Name();
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());
    }
  }
}
