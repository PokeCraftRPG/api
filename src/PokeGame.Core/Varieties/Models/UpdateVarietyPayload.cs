using FluentValidation;

namespace PokeGame.Core.Varieties.Models;

public record UpdateVarietyPayload
{
  public bool? IsDefault { get; set; }

  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public bool? CanChangeForm { get; set; }
  public Optional<int?>? GenderRatio { get; set; }
  public Optional<string>? Genus { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateVarietyPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => x.GenderRatio?.Value is not null, () => RuleFor(x => x.GenderRatio!.Value!.Value).GenderRatio());
      When(x => !string.IsNullOrWhiteSpace(x.Genus?.Value), () => RuleFor(x => x.Genus!.Value!).Genus());
    }
  }
}
