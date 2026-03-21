using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Abilities.Models;

public record UpdateAbilityPayload
{
  // TODO(fpion): Key/Slug
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateAbilityPayload>
  {
    public Validator()
    {
      // TODO(fpion): Key/Slug
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());
    }
  }
}
