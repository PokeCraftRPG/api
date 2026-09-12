using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Evolutions.Models;

public record SearchEvolutionsPayload : SearchPayload<EvolutionSort>
{
  public string? Source { get; set; }
  public string? Target { get; set; }
  public EvolutionTrigger? Trigger { get; set; }

  public override void Validate() => new Validator().ValidateQueryAndThrow(this);

  private class Validator : AbstractValidator<SearchEvolutionsPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<EvolutionSort>());

      RuleFor(x => x.Source).MaximumLength(Key.MaximumLength);
      RuleFor(x => x.Target).MaximumLength(Key.MaximumLength);
      RuleFor(x => x.Trigger).IsInEnum();
    }
  }
}
