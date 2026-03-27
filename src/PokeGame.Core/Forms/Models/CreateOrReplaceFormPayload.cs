using FluentValidation;
using PokeGame.Core.Forms.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Forms.Models;

public record CreateOrReplaceFormPayload
{
  public string Variety { get; set; }
  public bool IsDefault { get; set; }

  public string Key { get; set; }
  public string? Name { get; set; }
  public string? Description { get; set; }

  public bool IsBattleOnly { get; set; }
  public bool IsMega { get; set; }

  public int Height { get; set; }
  public int Weight { get; set; }

  public TypesModel Types { get; set; } = new();
  public AbilitiesPayload Abilities { get; set; } = new();
  public BaseStatisticsModel BaseStatistics { get; set; } = new();
  public YieldModel Yield { get; set; } = new();
  public SpritesModel Sprites { get; set; } = new();

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public CreateOrReplaceFormPayload() : this(string.Empty, string.Empty, default, default)
  {
  }

  public CreateOrReplaceFormPayload(string variety, string key, int height, int weight)
  {
    Variety = variety;
    Key = key;
    Height = height;
    Weight = weight;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceFormPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Variety).NotEmpty();

      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      RuleFor(x => x.Height).Height();
      RuleFor(x => x.Weight).Weight();

      RuleFor(x => x.Types).SetValidator(new TypesValidator());
      RuleFor(x => x.Abilities).SetValidator(new AbilitiesValidator());
      RuleFor(x => x.BaseStatistics).SetValidator(new BaseStatisticsValidator());
      RuleFor(x => x.Yield).SetValidator(new YieldValidator());
      RuleFor(x => x.Sprites).SetValidator(new SpritesValidator());

      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());
    }
  }
}
