using FluentValidation;

namespace PokeGame.Core.Forms.Models;

public record UpdateFormPayload
{
  public string? Key { get; set; }

  public Optional<string>? Name { get; set; }
  public Optional<string>? Summary { get; set; }
  public Optional<string>? Content { get; set; }

  public FormTypesDto? Types { get; set; }
  public FormAbilitiesPayload? Abilities { get; set; }
  public BaseStatisticsDto? BaseStatistics { get; set; }
  public FormYieldDto? Yield { get; set; }

  public Optional<FormSizeDto?>? Size { get; set; }
  public Optional<FormSpritesPayload?>? Sprites { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateFormPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());

      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary?.Value), () => RuleFor(x => x.Summary!.Value!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content?.Value), () => RuleFor(x => x.Content!.Value!).Content());

      When(x => x.Types is not null, () => RuleFor(x => x.Types!).SetValidator(new FormTypesValidator()));
      When(x => x.Abilities is not null, () => RuleFor(x => x.Abilities!).SetValidator(new FormAbilitiesPayloadValidator()));
      When(x => x.BaseStatistics is not null, () => RuleFor(x => x.BaseStatistics!).SetValidator(new BaseStatisticsValidator()));
      When(x => x.Yield is not null, () => RuleFor(x => x.Yield!).SetValidator(new FormYieldValidator()));

      When(x => x.Size?.Value is not null, () => RuleFor(x => x.Size!.Value!).SetValidator(new FormSizeValidator()));
      When(x => x.Sprites?.Value is not null, () => RuleFor(x => x.Sprites!.Value!).SetValidator(new FormSpritesPayloadValidator()));
    }
  }
}
