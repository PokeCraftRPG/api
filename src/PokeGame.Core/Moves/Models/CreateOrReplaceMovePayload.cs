using FluentValidation;

namespace PokeGame.Core.Moves.Models;

public record CreateOrReplaceMovePayload
{
  public PokemonType Type { get; set; }
  public MoveCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int? Accuracy { get; set; }
  public int? Power { get; set; }
  public int? PowerPoints { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceMovePayload>
  {
    public Validator()
    {
      RuleFor(x => x.Type).IsInEnum();
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Key();

      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      When(x => x.Accuracy.HasValue, () => RuleFor(x => x.Accuracy!.Value).Accuracy());
      When(x => x.Power.HasValue, () => RuleFor(x => x.Power!.Value).Power());
      When(x => x.PowerPoints.HasValue, () => RuleFor(x => x.PowerPoints!.Value).PowerPoints());
    }
  }
}
