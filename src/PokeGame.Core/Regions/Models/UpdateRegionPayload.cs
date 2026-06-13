using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Regions.Models;

public record UpdateRegionPayload
{
  public string? Key { get; set; } = string.Empty;
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateRegionPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());
    }
  }
}
