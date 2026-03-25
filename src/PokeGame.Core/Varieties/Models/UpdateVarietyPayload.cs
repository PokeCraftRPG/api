using FluentValidation;
using PokeGame.Core.Validation;
using PokeGame.Core.Varieties.Validators;

namespace PokeGame.Core.Varieties.Models;

public record UpdateVarietyPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Genus { get; set; }
  public Optional<string>? Description { get; set; }

  public Optional<int?>? GenderRatio { get; set; }

  public bool? CanChangeForm { get; set; }

  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public List<VarietyMovePayload> Moves { get; set; } = [];

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateVarietyPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Genus?.Value), () => RuleFor(x => x.Genus!.Value!).Genus());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.GenderRatio?.Value is not null, () => RuleFor(x => x.GenderRatio!.Value!.Value).GenderRatio());

      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());

      RuleForEach(x => x.Moves).SetValidator(new VarietyMoveValidator(allowNullLevel: true));
    }
  }
}
