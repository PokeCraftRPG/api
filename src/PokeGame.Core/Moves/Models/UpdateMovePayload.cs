using FluentValidation;

namespace PokeGame.Core.Moves.Models;

public record UpdateMovePayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public Optional<int?>? Accuracy { get; set; }
  public Optional<int?>? Power { get; set; }
  public Optional<int?>? PowerPoints { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateMovePayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => x.Accuracy?.Value is not null, () => RuleFor(x => x.Accuracy!.Value!.Value).Accuracy());
      When(x => x.Power?.Value is not null, () => RuleFor(x => x.Power!.Value!.Value).Power());
      When(x => x.PowerPoints?.Value is not null, () => RuleFor(x => x.PowerPoints!.Value!.Value).PowerPoints());
    }
  }
}
