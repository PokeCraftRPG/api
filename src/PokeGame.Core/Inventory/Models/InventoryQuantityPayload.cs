using FluentValidation;

namespace PokeGame.Core.Inventory.Models;

public record InventoryQuantityPayload
{
  public int Quantity { get; set; }

  public InventoryQuantityPayload(int quantity = 0)
  {
    Quantity = quantity;
  }

  public void Validate(bool allowZero = false) => new Validator(allowZero).ValidateAndThrow(this);

  private class Validator : AbstractValidator<InventoryQuantityPayload>
  {
    public Validator(bool allowZero)
    {
      if (allowZero)
      {
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
      }
      else
      {
        RuleFor(x => x.Quantity).GreaterThan(0);
      }
    }
  }
}
