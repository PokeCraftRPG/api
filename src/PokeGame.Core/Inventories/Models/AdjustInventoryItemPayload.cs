using FluentValidation;

namespace PokeGame.Core.Inventories.Models;

public record AdjustInventoryItemPayload
{
  public int Delta { get; set; }

  public void Validate() => new Validator().ValidateCommandAndThrow(this);

  private class Validator : AbstractValidator<AdjustInventoryItemPayload>
  {
    public Validator()
    {
      RuleFor(x => x.Delta).InclusiveBetween(-Inventory.MaximumQuantity, Inventory.MaximumQuantity).NotEqual(0);
    }
  }
}
