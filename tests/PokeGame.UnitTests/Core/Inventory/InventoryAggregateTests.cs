using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Inventory;

[Trait(Traits.Category, Categories.Unit)]
public class InventoryAggregateTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Trainer _trainer;
  private readonly Item _item;
  private readonly InventoryAggregate _inventory;

  public InventoryAggregateTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _item = new ItemBuilder(_faker).WithWorld(_world).Build();
    _inventory = new InventoryAggregate(_trainer);
  }

  [Fact(DisplayName = "Add: it should throw ArgumentOutOfRangeException when the quantity is negative.")]
  public void Given_NegativeQuantity_When_Add_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Add(_item, -3, _world.OwnerId));
    Assert.Equal("quantity", exception.ParamName);
  }

  [Fact(DisplayName = "Add: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_WorldMismatch_When_Add_Then_WorldMismatchException()
  {
    Item item = new ItemBuilder().Build();
    var exception = Assert.Throws<WorldMismatchException>(() => _inventory.Add(item, quantity: 1, _world.OwnerId));
    Assert.Equal(_inventory.Id.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("itemId", exception.ParamName);
  }

  [Fact(DisplayName = "EnsureQuantity: it should not throw when the item quantity is enough.")]
  public void Given_QuantityIsEqual_When_EnsureQuantity_Then_NoException()
  {
    int quantity = _faker.Random.Int(1, 999);
    _inventory.Add(_item, quantity, _world.OwnerId);
    _inventory.EnsureQuantity(_item, quantity);
  }

  [Fact(DisplayName = "EnsureQuantity: it should not throw when the item quantity is greater.")]
  public void Given_QuantityIsGreater_When_EnsureQuantity_Then_NoException()
  {
    int existingQuantity = _faker.Random.Int(2, 999);
    int quantity = existingQuantity - 1;
    _inventory.Add(_item, existingQuantity, _world.OwnerId);
    _inventory.EnsureQuantity(_item, quantity);
  }

  [Theory(DisplayName = "EnsureQuantity: it should throw InsufficientInventoryQuantityException when the item quantity is not enough.")]
  [InlineData(0)]
  [InlineData(5)]
  public void Given_QuantityIsLess_When_EnsureQuantity_Then_InsufficientInventoryQuantityException(int quantity)
  {
    _inventory.Add(_item, quantity, _world.OwnerId);
    int requiredQuantity = quantity + _faker.Random.Int(1, 10);

    var exception = Assert.Throws<InsufficientInventoryQuantityException>(() => _inventory.EnsureQuantity(_item, requiredQuantity));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.Equal(_item.EntityId, exception.ItemId);
    Assert.Equal(quantity, exception.AvailableQuantity);
    Assert.Equal(requiredQuantity, exception.RequiredQuantity);
  }

  [Fact(DisplayName = "GetQuantity: it should return 0 when the item is not present.")]
  public void Given_ItemIsNotPresent_When_GetQuantity_Then_ZeroQuantity()
  {
    Assert.Equal(0, _inventory.GetQuantity(_item));
  }

  [Fact(DisplayName = "GetQuantity: it should return the correct quantity when the item is present.")]
  public void Given_ItemIsPresent_When_GetQuantity_Then_CorrectQuantity()
  {
    int quantity = _faker.Random.Int(1, 999);
    _inventory.Add(_item, quantity, _world.OwnerId);
    Assert.Equal(quantity, _inventory.GetQuantity(_item));
  }

  [Fact(DisplayName = "HasQuantity: it should return false when the item is not present.")]
  public void Given_ItemIsNotPresent_When_HasQuantity_Then_FalseReturned()
  {
    int quantity = _faker.Random.Int(1, 999);
    Assert.False(_inventory.HasQuantity(_item, quantity));
  }

  [Fact(DisplayName = "HasQuantity: it should return true when the item quantity is enough.")]
  public void Given_QuantityIsEqual_When_HasQuantity_Then_TrueReturned()
  {
    int quantity = _faker.Random.Int(1, 999);
    _inventory.Add(_item, quantity, _world.OwnerId);
    Assert.True(_inventory.HasQuantity(_item, quantity));
  }

  [Fact(DisplayName = "HasQuantity: it should return true when the item quantity is greater.")]
  public void Given_QuantityIsGreater_When_HasQuantity_Then_TrueReturned()
  {
    int existingQuantity = _faker.Random.Int(2, 999);
    int quantity = existingQuantity - 1;
    _inventory.Add(_item, existingQuantity, _world.OwnerId);
    Assert.True(_inventory.HasQuantity(_item, quantity));
  }

  [Fact(DisplayName = "Remove: it should throw ArgumentOutOfRangeException when the quantity is negative.")]
  public void Given_NegativeQuantity_When_Remove_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Remove(_item, -2, _world.OwnerId));
    Assert.Equal("quantity", exception.ParamName);
  }

  [Fact(DisplayName = "Remove: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_WorldMismatch_When_Remove_Then_WorldMismatchException()
  {
    Item item = new ItemBuilder().Build();
    var exception = Assert.Throws<WorldMismatchException>(() => _inventory.Remove(item, quantity: 1, _world.OwnerId));
    Assert.Equal(_inventory.Id.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("itemId", exception.ParamName);
  }

  [Fact(DisplayName = "Update: it should throw ArgumentOutOfRangeException when the quantity is negative.")]
  public void Given_NegativeQuantity_When_Update_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Update(_item, -1, _world.OwnerId));
    Assert.Equal("quantity", exception.ParamName);
  }

  [Fact(DisplayName = "Update: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_WorldMismatch_When_Update_Then_WorldMismatchException()
  {
    Item item = new ItemBuilder().Build();
    var exception = Assert.Throws<WorldMismatchException>(() => _inventory.Update(item, quantity: 1, _world.OwnerId));
    Assert.Equal(_inventory.Id.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("itemId", exception.ParamName);
  }
}
