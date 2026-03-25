using FluentValidation;
using PokeGame.Core.Species.Validators;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species.Models;

public record CreateOrReplaceSpeciesPayload
{
  public int Number { get; set; }
  public PokemonCategory Category { get; set; }

  public string Key { get; set; }
  public string? Name { get; set; }

  public byte BaseFriendship { get; set; }
  public byte CatchRate { get; set; }
  public GrowthRate GrowthRate { get; set; }

  public byte EggCycles { get; set; }
  public EggGroupsModel EggGroups { get; set; } = new();

  public string? Url { get; set; }
  public string? Notes { get; set; }

  public List<RegionalNumberPayload> RegionalNumbers { get; set; } = [];

  public CreateOrReplaceSpeciesPayload() : this(default, string.Empty, default, default)
  {
  }

  public CreateOrReplaceSpeciesPayload(int number, string key, byte catchRate, byte eggCycles)
  {
    Number = number;
    Key = key;
    CatchRate = catchRate;
    EggCycles = eggCycles;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceSpeciesPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Number).Number();
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());

      RuleFor(x => x.CatchRate).CatchRate();
      RuleFor(x => x.GrowthRate).IsInEnum();

      RuleFor(x => x.EggCycles).EggCycles();
      RuleFor(x => x.EggGroups).SetValidator(new EggGroupsValidator());

      When(x => !string.IsNullOrWhiteSpace(x.Url), () => RuleFor(x => x.Url!).Url());
      When(x => !string.IsNullOrWhiteSpace(x.Notes), () => RuleFor(x => x.Notes!).Notes());

      RuleForEach(x => x.RegionalNumbers).SetValidator(new RegionalNumberValidator());
    }
  }
}
