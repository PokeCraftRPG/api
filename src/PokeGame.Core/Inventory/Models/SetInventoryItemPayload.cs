using FluentValidation;

namespace PokeGame.Core.Inventory.Models;

public record SetInventoryItemPayload
{
  public int Quantity { get; set; }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<SetInventoryItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Quantity).InclusiveBetween(TrainerInventory.MinimumQuantity, TrainerInventory.MaximumQuantity);
    }
  }
}
