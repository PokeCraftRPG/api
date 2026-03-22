using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Worlds.Models;

public record UpdateWorldPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateWorldPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());
    }
  }
}
