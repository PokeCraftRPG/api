using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateInventoryItemCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IInventoryRepository> _inventoryRepository = new();
  private readonly Mock<IItemQuerier> _itemQuerier = new();
  private readonly Mock<IItemRepository> _itemRepository = new();
  private readonly Mock<ITrainerRepository> _trainerRepository = new();

  private readonly TestContext _context;
  private readonly UpdateInventoryItemCommandHandler _handler;

  public UpdateInventoryItemCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _inventoryRepository.Object, _itemQuerier.Object, _itemRepository.Object, _trainerRepository.Object);
  }

  [Theory(DisplayName = "It should update item(s) to the inventory.")]
  [InlineData(0, null)]
  [InlineData(0, 2)]
  [InlineData(5, null)]
  [InlineData(5, 0)]
  public async Task Given_TrainerAndItem_When_HandleAsync_Then_InventoryItemUpdated(int quantity, int? existingQuantity)
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    Item item = new ItemBuilder(_faker).WithWorld(_context.World).Build();
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    InventoryAggregate? inventory = null;
    if (existingQuantity.HasValue)
    {
      inventory = new(trainer);
      inventory.Add(item, existingQuantity.Value, _context.UserId);
      _inventoryRepository.Setup(x => x.LoadAsync(inventory.Id, _cancellationToken)).ReturnsAsync(inventory);
    }

    ItemModel model = new();
    _itemQuerier.Setup(x => x.ReadAsync(item, _cancellationToken)).ReturnsAsync(model);

    InventoryQuantityPayload payload = new(quantity);
    UpdateInventoryItemCommand command = new(trainer.EntityId, item.EntityId, payload);

    InventoryItemModel result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, result.Item);
    Assert.Equal(quantity, result.Quantity);

    if (inventory is null)
    {
      _inventoryRepository.Verify(x => x.SaveAsync(
        It.Is<InventoryAggregate>(i => i.TrainerId == trainer.Id && i.GetQuantity(item) == quantity),
        _cancellationToken), Times.Once());
    }
    else
    {
      if (quantity < 1)
      {
        Assert.Empty(inventory.Quantities);
      }
      else
      {
        Assert.Equal(quantity, inventory.GetQuantity(item));
      }
      _inventoryRepository.Verify(x => x.SaveAsync(inventory, _cancellationToken), Times.Once());
    }
  }

  [Fact(DisplayName = "It should throw ItemNotFoundException when the item was not found.")]
  public async Task Given_ItemNotFound_When_HandleAsync_Then_ItemNotFoundException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    InventoryQuantityPayload payload = new(quantity: 1);
    UpdateInventoryItemCommand command = new(trainer.EntityId, Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<ItemNotFoundException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(command.ItemId.ToString(), exception.Item);
    Assert.Equal("ItemId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw TrainerNotFoundException when the trainer was not found.")]
  public async Task Given_TrainerNotFound_When_HandleAsync_Then_TrainerNotFoundException()
  {
    InventoryQuantityPayload payload = new(quantity: 1);
    UpdateInventoryItemCommand command = new(Guid.NewGuid(), Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<TrainerNotFoundException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(command.TrainerId.ToString(), exception.Trainer);
    Assert.Equal("TrainerId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    InventoryQuantityPayload payload = new(quantity: -10);
    UpdateInventoryItemCommand command = new(Guid.NewGuid(), Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanOrEqualValidator" && e.PropertyName == "Quantity");
  }
}
