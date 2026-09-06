using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Forms.Models;

public record SearchFormsPayload : SearchPayload<FormSort>
{
  public string? Variety { get; set; }
  public FormCategory? Category { get; set; }
  public PokemonType? Type { get; set; }
  public string? Ability { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchFormsPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<FormSort>());

      RuleFor(x => x.Variety).MaximumLength(Key.MaximumLength);
      RuleFor(x => x.Category).IsInEnum();
      RuleFor(x => x.Type).IsInEnum();
      RuleFor(x => x.Ability).MaximumLength(Key.MaximumLength);
    }
  }
}
