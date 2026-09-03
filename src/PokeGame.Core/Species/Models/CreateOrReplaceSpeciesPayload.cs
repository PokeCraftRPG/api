using FluentValidation;

namespace PokeGame.Core.Species.Models;

public record CreateOrReplaceSpeciesPayload
{
  public int Number { get; set; }
  public SpeciesCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;

  public string? Name { get; set; }
  public string? Summary { get; set; }
  public string? Content { get; set; }

  public int BaseFriendship { get; set; }
  public int CatchRate { get; set; } = Species.CatchRate.MaximumValue;
  public GrowthRate GrowthRate { get; set; }
  public SpeciesEggsDto Eggs { get; set; } = new();

  public List<RegionalNumberPayload> RegionalNumbers { get; set; } = [];

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceSpeciesPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Number).Number();
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Key();

      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Summary), () => RuleFor(x => x.Summary!).Summary());
      When(x => !string.IsNullOrWhiteSpace(x.Content), () => RuleFor(x => x.Content!).Content());

      RuleFor(x => x.BaseFriendship).Friendship();
      RuleFor(x => x.CatchRate).CatchRate();
      RuleFor(x => x.GrowthRate).IsInEnum();
      RuleFor(x => x.Eggs).SetValidator(new SpeciesEggsValidator());

      RuleFor(x => x.RegionalNumbers).MaximumCount(20);
      RuleFor(x => x.RegionalNumbers).Must(regionalNumbers => regionalNumbers.Select(x => x.RegionId).Distinct().Count() == regionalNumbers.Count)
        .WithErrorCode("UniqueCollectionValidator")
        .WithMessage("'{PropertyName}' may not include duplicate regions.");
      RuleForEach(x => x.RegionalNumbers).SetValidator(new RegionalNumberValidator());
    }
  }
}
