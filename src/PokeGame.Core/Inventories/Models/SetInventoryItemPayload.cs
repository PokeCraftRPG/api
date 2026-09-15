using FluentValidation;

namespace PokeGame.Core.Inventories.Models;

public record SetInventoryItemPayload
{
  public int Quantity { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<SetInventoryItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Quantity).InclusiveBetween(Inventory.MinimumQuantity, Inventory.MaximumQuantity);
    }
  }
}
