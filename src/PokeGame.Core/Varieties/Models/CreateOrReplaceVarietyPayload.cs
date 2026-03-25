using FluentValidation;
using PokeGame.Core.Validation;
using PokeGame.Core.Varieties.Validators;

namespace PokeGame.Core.Varieties.Models;

public record CreateOrReplaceVarietyPayload
{
  public string Species { get; set; }
  public bool IsDefault { get; set; }

  public string Key { get; set; }
  public string? Name { get; set; }
  public string? Genus { get; set; }
  public string? Description { get; set; }

  public int? GenderRatio { get; set; }

  public bool CanChangeForm { get; set; }

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public List<VarietyMovePayload> Moves { get; set; } = [];

  public CreateOrReplaceVarietyPayload() : this(string.Empty, string.Empty)
  {
  }

  public CreateOrReplaceVarietyPayload(string species, string key)
  {
    Species = species;
    Key = key;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceVarietyPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Species).NotEmpty();

      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Genus), () => RuleFor(x => x.Genus!).Genus());
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      When(x => x.GenderRatio.HasValue, () => RuleFor(x => x.GenderRatio!.Value).GenderRatio());

      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());

      RuleForEach(x => x.Moves).SetValidator(new VarietyMoveValidator());
    }
  }
}
