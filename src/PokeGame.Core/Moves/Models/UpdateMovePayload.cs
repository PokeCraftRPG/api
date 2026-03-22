using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves.Models;

public record UpdateMovePayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public Optional<byte?>? Accuracy { get; set; }
  public Optional<byte?>? Power { get; set; }
  public byte? PowerPoints { get; set; }

  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateMovePayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.Accuracy?.Value is not null, () => RuleFor(x => x.Accuracy!.Value!.Value).Accuracy());
      When(x => x.Power?.Value is not null, () => RuleFor(x => x.Power!.Value!.Value).Power());
      When(x => x.PowerPoints.HasValue, () => RuleFor(x => x.PowerPoints!.Value).PowerPoints());

      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());
    }
  }
}
