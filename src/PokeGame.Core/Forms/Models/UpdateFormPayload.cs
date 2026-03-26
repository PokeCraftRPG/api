using FluentValidation;
using PokeGame.Core.Forms.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Forms.Models;

public record UpdateFormPayload
{
  public bool? IsDefault { get; set; }

  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public bool? IsBattleOnly { get; set; }
  public bool? IsMega { get; set; }

  public int? Height { get; set; }
  public int? Weight { get; set; }

  public TypesModel? Types { get; set; }
  // TODO(fpion): Abilities
  // TODO(fpion): BaseStatistics
  public YieldModel? Yield { get; set; }
  public SpritesModel? Sprites { get; set; }

  public Optional<string>? Url { get; set; }
  public Optional<string>? Note { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateFormPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.Height.HasValue, () => RuleFor(x => x.Height!.Value).Height());
      When(x => x.Weight.HasValue, () => RuleFor(x => x.Weight!.Value).Weight());

      When(x => x.Types is not null, () => RuleFor(x => x.Types!).SetValidator(new TypesValidator()));
      // TODO(fpion): Abilities
      // TODO(fpion): BaseStatistics
      When(x => x.Yield is not null, () => RuleFor(x => x.Yield!).SetValidator(new YieldValidator()));
      When(x => x.Sprites is not null, () => RuleFor(x => x.Sprites!).SetValidator(new SpritesValidator()));

      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Note?.Value), () => RuleFor(x => x.Note!.Value!).Notes());
    }
  }
}
