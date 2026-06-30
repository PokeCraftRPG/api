using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species.Models;

public record CreateOrReplaceSpeciesPayload
{
  public int Number { get; set; }
  public PokemonCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public int BaseFriendship { get; set; }
  public int CatchRate { get; set; }
  public GrowthRate GrowthRate { get; set; }

  public int EggCycles { get; set; }
  public EggGroupsModel EggGroups { get; set; } = new();
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
      When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description!).Description());

      RuleFor(x => x.BaseFriendship).Friendship();
      RuleFor(x => x.CatchRate).CatchRate();
      RuleFor(x => x.GrowthRate).IsInEnum();

      RuleFor(x => x.EggCycles).EggCycles();
      RuleFor(x => x.EggGroups).SetValidator(new EggGroupsValidator());

      RuleForEach(x => x.RegionalNumbers).SetValidator(new RegionalNumberValidator());
    }
  }
}
