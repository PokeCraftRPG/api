using FluentValidation;

namespace PokeGame.Core.Varieties.Models;

public record CreateOrReplaceVarietyPayload
{
  public Guid SpeciesId { get; set; }
  public bool IsDefault { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public bool CanChangeForm { get; set; }
  public int? GenderRatio { get; set; }
  public string? Genus { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceVarietyPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Key).Key();

      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      When(x => x.GenderRatio.HasValue, () => RuleFor(x => x.GenderRatio!.Value).GenderRatio());
      When(x => !string.IsNullOrWhiteSpace(x.Genus), () => RuleFor(x => x.Genus!).Genus());
    }
  }
}
