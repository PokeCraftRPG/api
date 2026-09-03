using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Species.Models;

public record SearchSpeciesPayload : SearchPayload<SpeciesSort>
{
  public SpeciesCategory? Category { get; set; }
  public GrowthRate? GrowthRate { get; set; }
  public EggGroup? EggGroup { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchSpeciesPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<SpeciesSort>());

      RuleFor(x => x.Category).IsInEnum();
      RuleFor(x => x.GrowthRate).IsInEnum();
      RuleFor(x => x.EggGroup).IsInEnum();
    }
  }
}
