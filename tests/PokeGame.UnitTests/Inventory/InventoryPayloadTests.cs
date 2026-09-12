using PokeGame.Core;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Inventory;

public class InventoryPayloadTests : UnitTests
{
  [Theory(DisplayName = "It should throw InvalidCommandException when the set quantity is out of range.")]
  [InlineData(-1)]
  [InlineData(1000)]
  public void Given_QuantityOutOfRange_When_ValidateSet_Then_InvalidCommandException(int quantity)
  {
    SetInventoryItemPayload payload = new() { Quantity = quantity };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should accept a valid set quantity.")]
  public void Given_ValidQuantity_When_ValidateSet_Then_Valid()
  {
    new SetInventoryItemPayload { Quantity = TrainerInventory.MaximumQuantity }.Validate();
  }

  [Theory(DisplayName = "It should throw InvalidCommandException when the adjust delta is invalid.")]
  [InlineData(0)]
  [InlineData(1000)]
  [InlineData(-1000)]
  public void Given_InvalidDelta_When_ValidateAdjust_Then_InvalidCommandException(int delta)
  {
    AdjustInventoryItemPayload payload = new() { Delta = delta };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should accept a valid adjust delta.")]
  public void Given_ValidDelta_When_ValidateAdjust_Then_Valid()
  {
    new AdjustInventoryItemPayload { Delta = 1 }.Validate();
  }
}
