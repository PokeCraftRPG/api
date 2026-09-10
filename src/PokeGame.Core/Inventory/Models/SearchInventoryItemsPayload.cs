using FluentValidation;
using PokeGame.Core.Items;
using PokeGame.Core.Search;

namespace PokeGame.Core.Inventory.Models;

public record SearchInventoryItemsPayload : SearchPayload<InventoryItemSort>
{
  public ItemCategory? Category { get; set; }

  public override void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SearchInventoryItemsPayload>
  {
    public Validator()
    {
      Include(new SearchValidator<InventoryItemSort>());

      RuleFor(x => x.Category).IsInEnum();
    }
  }
}
