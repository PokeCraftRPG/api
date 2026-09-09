using FluentValidation;
using PokeGame.Core.Search;

namespace PokeGame.Core.Items.Models;

public record SearchItemsPayload : SearchPayload<ItemSort>
{
  public ItemCategory? Category { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchItemsPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<ItemSort>());

      RuleFor(x => x.Category).IsInEnum();
    }
  }
}
