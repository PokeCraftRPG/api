using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Events;
using PokeGame.Core.Items;

namespace PokeGame.Inventory;

public class TrainerInventoryTests : UnitTests
{
  [Fact(DisplayName = "It should add an item when the quantity goes from 0 to a positive value.")]
  public void Given_MissingItem_When_SetPositive_Then_InventoryItemAdded()
  {
    TrainerInventory inventory = new(Catalog.Red);

    inventory.SetQuantity(Catalog.Potion, 5);

    Assert.Equal(5, inventory.Quantities[Catalog.Potion.Id]);
    InventoryItemAdded @event = inventory.LastChange<InventoryItemAdded>();
    Assert.Equal(Catalog.Potion.Id, @event.ItemId);
    Assert.Equal(5, @event.Quantity);
  }

  [Fact(DisplayName = "It should change the quantity of an existing item.")]
  public void Given_ExistingItem_When_SetDifferentQuantity_Then_InventoryItemChanged()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 5);
    inventory.ClearChanges();

    inventory.SetQuantity(Catalog.Potion, 12);

    Assert.Equal(12, inventory.Quantities[Catalog.Potion.Id]);
    InventoryItemChanged @event = inventory.LastChange<InventoryItemChanged>();
    Assert.Equal(Catalog.Potion.Id, @event.ItemId);
    Assert.Equal(12, @event.Quantity);
  }

  [Fact(DisplayName = "It should remove an item when the quantity is set to 0.")]
  public void Given_ExistingItem_When_SetZero_Then_InventoryItemRemoved()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 8);
    inventory.ClearChanges();

    inventory.SetQuantity(Catalog.Potion, 0);

    Assert.False(inventory.Quantities.ContainsKey(Catalog.Potion.Id));
    InventoryItemRemoved @event = inventory.LastChange<InventoryItemRemoved>();
    Assert.Equal(Catalog.Potion.Id, @event.ItemId);
  }

  [Fact(DisplayName = "It should not raise an event when the quantity does not change.")]
  public void Given_SameQuantity_When_Set_Then_NoEvent()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 7);
    inventory.ClearChanges();

    inventory.SetQuantity(Catalog.Potion, 7);

    Assert.False(inventory.HasChanges);
    Assert.Equal(7, inventory.Quantities[Catalog.Potion.Id]);
  }

  [Fact(DisplayName = "It should not raise an event when setting 0 for a missing item.")]
  public void Given_MissingItem_When_SetZero_Then_NoEvent()
  {
    TrainerInventory inventory = new(Catalog.Red);

    inventory.SetQuantity(Catalog.Potion, 0);

    Assert.False(inventory.HasChanges);
    Assert.Empty(inventory.Quantities);
  }

  [Fact(DisplayName = "It should set the maximum quantity.")]
  public void Given_MaximumQuantity_When_Set_Then_Set()
  {
    TrainerInventory inventory = new(Catalog.Red);

    inventory.SetQuantity(Catalog.Potion, TrainerInventory.MaximumQuantity);

    Assert.Equal(TrainerInventory.MaximumQuantity, inventory.Quantities[Catalog.Potion.Id]);
  }

  [Fact(DisplayName = "It should add an item when adjusting a positive delta.")]
  public void Given_MissingItem_When_AdjustPositive_Then_Added()
  {
    TrainerInventory inventory = new(Catalog.Red);

    inventory.AdjustQuantity(Catalog.Potion, 4);

    Assert.Equal(4, inventory.Quantities[Catalog.Potion.Id]);
    Assert.IsType<InventoryItemAdded>(inventory.LastChange<InventoryItemAdded>());
  }

  [Fact(DisplayName = "It should increase an existing quantity.")]
  public void Given_ExistingItem_When_AdjustPositive_Then_Increased()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 10);
    inventory.ClearChanges();

    inventory.AdjustQuantity(Catalog.Potion, 5);

    Assert.Equal(15, inventory.Quantities[Catalog.Potion.Id]);
    Assert.Equal(15, inventory.LastChange<InventoryItemChanged>().Quantity);
  }

  [Fact(DisplayName = "It should decrease an existing quantity.")]
  public void Given_ExistingItem_When_AdjustNegative_Then_Decreased()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 10);

    inventory.AdjustQuantity(Catalog.Potion, -3);

    Assert.Equal(7, inventory.Quantities[Catalog.Potion.Id]);
  }

  [Fact(DisplayName = "It should remove an item when adjusting the quantity to 0.")]
  public void Given_ExistingItem_When_AdjustToZero_Then_Removed()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 6);

    inventory.AdjustQuantity(Catalog.Potion, -6);

    Assert.False(inventory.Quantities.ContainsKey(Catalog.Potion.Id));
    Assert.IsType<InventoryItemRemoved>(inventory.Changes.Last());
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when the delta is 0.")]
  public void Given_ZeroDelta_When_Adjust_Then_ArgumentOutOfRangeException()
  {
    TrainerInventory inventory = new(Catalog.Red);

    Assert.Throws<ArgumentOutOfRangeException>(() => inventory.AdjustQuantity(Catalog.Potion, 0));
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when the quantity would exceed the maximum.")]
  public void Given_WouldExceedMaximum_When_Adjust_Then_InventoryQuantityOutOfRangeException()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, TrainerInventory.MaximumQuantity);

    InventoryQuantityOutOfRangeException exception = Assert.Throws<InventoryQuantityOutOfRangeException>(
      () => inventory.AdjustQuantity(Catalog.Potion, 1));
    Assert.Equal(Catalog.World.Id.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(Catalog.Potion.EntityId, exception.Data["ItemId"]);
    Assert.Equal(TrainerInventory.MinimumQuantity, exception.Data["MinimumQuantity"]);
    Assert.Equal(TrainerInventory.MaximumQuantity, exception.Data["MaximumQuantity"]);
    Assert.Equal(TrainerInventory.MaximumQuantity + 1, exception.Data["AttemptedQuantity"]);
    Assert.Equal("Quantity", exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when the quantity would go below the minimum.")]
  public void Given_WouldGoBelowMinimum_When_Adjust_Then_InventoryQuantityOutOfRangeException()
  {
    TrainerInventory inventory = new(Catalog.Red);

    InventoryQuantityOutOfRangeException exception = Assert.Throws<InventoryQuantityOutOfRangeException>(
      () => inventory.AdjustQuantity(Catalog.Potion, -1));
    Assert.Equal(-1, exception.Data["AttemptedQuantity"]);
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when setting a quantity above the maximum.")]
  public void Given_AboveMaximum_When_Set_Then_InventoryQuantityOutOfRangeException()
  {
    TrainerInventory inventory = new(Catalog.Red);

    Assert.Throws<InventoryQuantityOutOfRangeException>(
      () => inventory.SetQuantity(Catalog.Potion, TrainerInventory.MaximumQuantity + 1));
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the item belongs to another world.")]
  public void Given_DifferentWorld_When_Set_Then_WorldMismatchException()
  {
    TrainerInventory inventory = new(Catalog.Red);
    Item otherPotion = ItemBuilder.Potion(Faker, Catalog.OtherWorld());

    Assert.Throws<WorldMismatchException>(() => inventory.SetQuantity(otherPotion, 1));
  }

  [Fact(DisplayName = "It should restore quantities after replaying uncommitted events.")]
  public void Given_Changes_When_Replay_Then_StateRestored()
  {
    TrainerInventory inventory = new(Catalog.Red);
    inventory.SetQuantity(Catalog.Potion, 5);
    inventory.SetQuantity(Catalog.MasterBall, 2);
    inventory.SetQuantity(Catalog.Potion, 9);
    inventory.SetQuantity(Catalog.MasterBall, 0);

    TrainerInventory replayed = inventory.Replay();

    Assert.Equal(9, replayed.Quantities[Catalog.Potion.Id]);
    Assert.False(replayed.Quantities.ContainsKey(Catalog.MasterBall.Id));
  }
}
