using FluentValidation;
using PokeGame.Core.Species.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species.Models;

public record UpdateSpeciesPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }

  public byte? BaseFriendship { get; set; }
  public byte? CatchRate { get; set; }
  public GrowthRate? GrowthRate { get; set; }

  public byte? EggCycles { get; set; }
  public EggGroupsModel? EggGroups { get; set; }

  public Optional<string>? Url { get; set; }
  public Optional<string>? Notes { get; set; }

  public List<RegionalNumberPayload> RegionalNumbers { get; set; } = [];

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateSpeciesPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Slug());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());

      When(x => x.CatchRate.HasValue, () => RuleFor(x => x.CatchRate!.Value).CatchRate());
      When(x => x.GrowthRate.HasValue, () => RuleFor(x => x.GrowthRate!.Value).IsInEnum());

      When(x => x.EggCycles.HasValue, () => RuleFor(x => x.EggCycles!.Value).EggCycles());
      When(x => x.EggGroups is not null, () => RuleFor(x => x.EggGroups!).SetValidator(new EggGroupsValidator()));

      When(x => !string.IsNullOrWhiteSpace(x.Url?.Value), () => RuleFor(x => x.Url!.Value!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes?.Value), () => RuleFor(x => x.Notes!.Value!).Notes());

      RuleForEach(x => x.RegionalNumbers).SetValidator(new RegionalNumberValidator(minimum: 0));
    }
  }
}
