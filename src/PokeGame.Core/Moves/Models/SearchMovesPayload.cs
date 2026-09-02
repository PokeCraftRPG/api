using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Moves.Models;

public record SearchMovesPayload : SearchPayload<MoveSort>
{
  public PokemonType? Type { get; set; }
  public MoveCategory? Category { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchMovesPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<MoveSort>());

      RuleFor(x => x.Type).IsInEnum();
      RuleFor(x => x.Category).IsInEnum();
    }
  }
}
