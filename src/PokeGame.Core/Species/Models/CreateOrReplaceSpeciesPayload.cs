using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species.Models;

public record CreateOrReplaceSpeciesPayload
{
  public PokemonCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<CreateOrReplaceSpeciesPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Category).IsInEnum();

      RuleFor(x => x.Key).Slug();
      When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name!).Name());
    }
  }
}
