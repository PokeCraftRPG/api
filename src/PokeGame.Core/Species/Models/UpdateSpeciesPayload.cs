using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species.Models;

public record UpdateSpeciesPayload
{
  public string? Key { get; set; }
  public Optional<string>? Name { get; set; }
  public Optional<string>? Description { get; set; }

  public int? BaseFriendship { get; set; }
  public int? CatchRate { get; set; }
  public GrowthRate? GrowthRate { get; set; }

  public int? EggCycles { get; set; }
  public EggGroupsModel? EggGroups { get; set; }

  public List<RegionalNumberPayload> RegionalNumbers { get; set; } = [];

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UpdateSpeciesPayload>
  {
    public Validator()
    {
      When(x => !string.IsNullOrWhiteSpace(x.Key), () => RuleFor(x => x.Key!).Key());
      When(x => !string.IsNullOrWhiteSpace(x.Name?.Value), () => RuleFor(x => x.Name!.Value!).Name());
      When(x => !string.IsNullOrWhiteSpace(x.Description?.Value), () => RuleFor(x => x.Description!.Value!).Description());

      When(x => x.BaseFriendship.HasValue, () => RuleFor(x => x.BaseFriendship!.Value).Friendship());
      When(x => x.CatchRate.HasValue, () => RuleFor(x => x.CatchRate!.Value).CatchRate());
      When(x => x.GrowthRate.HasValue, () => RuleFor(x => x.GrowthRate!.Value).IsInEnum());

      When(x => x.EggCycles.HasValue, () => RuleFor(x => x.EggCycles!.Value).EggCycles());
      When(x => x.EggGroups is not null, () => RuleFor(x => x.EggGroups!).SetValidator(new EggGroupsValidator()));

      RuleForEach(x => x.RegionalNumbers).SetValidator(new RegionalNumberValidator(allowZero: true));
    }
  }
}
