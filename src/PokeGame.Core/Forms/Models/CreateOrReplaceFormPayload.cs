using FluentValidation;

namespace PokeGame.Core.Forms.Models;

public record CreateOrReplaceFormPayload
{
  public Guid VarietyId { get; set; }
  public FormCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public FormTypesDto Types { get; set; } = new();
  public FormAbilitiesPayload Abilities { get; set; } = new();
  public BaseStatisticsDto BaseStatistics { get; set; } = new();
  public FormYieldDto Yield { get; set; } = new();

  public FormSizeDto? Size { get; set; }
  public FormSpritesPayload? Sprites { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceFormPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Key();

      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      RuleFor(x => x.Types).SetValidator(new FormTypesValidator());
      RuleFor(x => x.Abilities).SetValidator(new FormAbilitiesPayloadValidator());
      RuleFor(x => x.BaseStatistics).SetValidator(new BaseStatisticsValidator());
      RuleFor(x => x.Yield).SetValidator(new FormYieldValidator());

      When(x => x.Size is not null, () => RuleFor(x => x.Size!).SetValidator(new FormSizeValidator()));
      When(x => x.Sprites is not null, () => RuleFor(x => x.Sprites!).SetValidator(new FormSpritesPayloadValidator()));
    }
  }
}
