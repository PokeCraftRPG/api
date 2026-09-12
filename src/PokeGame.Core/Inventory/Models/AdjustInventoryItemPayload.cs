using FluentValidation;

namespace PokeGame.Core.Inventory.Models;

public record AdjustInventoryItemPayload
{
  public int Delta { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<AdjustInventoryItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Delta).InclusiveBetween(-TrainerInventory.MaximumQuantity, TrainerInventory.MaximumQuantity).NotEqual(0);
    }
  }
}
