using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Trainers;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class InventoryIntegrationTests : IntegrationTests
{
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IInventoryService _inventoryService;
  private readonly IItemRepository _itemRepository;
  private readonly ITrainerRepository _trainerRepository;

  private Trainer _trainer = null!;
  private Item _item = null!;

  public InventoryIntegrationTests()
  {
    _inventoryRepository = ServiceProvider.GetRequiredService<IInventoryRepository>();
    _inventoryService = ServiceProvider.GetRequiredService<IInventoryService>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = new TrainerBuilder(Faker).WithWorld(World).Build();
    await _trainerRepository.SaveAsync(_trainer);

    _item = ItemBuilder.Leftovers(Faker, World);
    await _itemRepository.SaveAsync(_item);
  }

  [Theory(DisplayName = "It should add quantity to the inventory.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_Payload_When_Add_Then_QuantityAdded(bool exists)
  {
    int existingQuantity = exists ? Faker.Random.Int(1, 10) : 0;
    if (existingQuantity > 0)
    {
      InventoryAggregate inventory = new(_trainer);
      inventory.Add(_item, existingQuantity, World.OwnerId);
      await _inventoryRepository.SaveAsync(inventory);
    }

    InventoryQuantityPayload payload = new(Faker.Random.Int(1, 10));
    InventoryItemModel result = await _inventoryService.AddAsync(_trainer.EntityId, _item.EntityId, payload);

    Assert.Equal(_item.EntityId, result.Item.Id);
    Assert.Equal(existingQuantity + payload.Quantity, result.Quantity);
  }

  [Theory(DisplayName = "It should remove quantity from the inventory.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_Payload_When_Remove_Then_QuantityRemoved(bool exists)
  {
    int existingQuantity = exists ? Faker.Random.Int(1, 10) : 0;
    if (existingQuantity > 0)
    {
      InventoryAggregate inventory = new(_trainer);
      inventory.Add(_item, existingQuantity, World.OwnerId);
      await _inventoryRepository.SaveAsync(inventory);
    }

    InventoryQuantityPayload payload = new(Faker.Random.Int(1, 10));
    InventoryItemModel result = await _inventoryService.RemoveAsync(_trainer.EntityId, _item.EntityId, payload);

    Assert.Equal(_item.EntityId, result.Item.Id);
    Assert.Equal(Math.Max(existingQuantity - payload.Quantity, 0), result.Quantity);
  }

  [Fact(DisplayName = "It should return null when the item is not in the inventory.")]
  public async Task Given_NoQuantity_When_Read_Then_NullReturned()
  {
    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));
  }

  [Fact(DisplayName = "It should return the inventory quantities.")]
  public async Task Given_Quantities_When_Search_Then_Inventory()
  {
    int quantity = Faker.Random.Int(1, 999);
    InventoryAggregate inventory = new(_trainer);
    inventory.Add(_item, quantity, World.OwnerId);
    await _inventoryRepository.SaveAsync(inventory);

    SearchResults<InventoryItemModel> results = await _inventoryService.SearchAsync(_trainer.EntityId);
    Assert.Equal(1, results.Total);
    Assert.Single(results.Items);
    Assert.Contains(results.Items, i => i.Item.Id == _item.EntityId && i.Quantity == quantity);
  }

  [Fact(DisplayName = "It should return the item and quantity when it is in the inventory.")]
  public async Task Given_Quantity_When_Read_Then_ItemAndQuantityReturned()
  {
    int quantity = Faker.Random.Int(1, 999);
    InventoryAggregate inventory = new(_trainer);
    inventory.Add(_item, quantity, World.OwnerId);
    await _inventoryRepository.SaveAsync(inventory);

    InventoryItemModel? result = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(result);
    Assert.Equal(_item.EntityId, result.Item.Id);
    Assert.Equal(quantity, result.Quantity);
  }

  [Theory(DisplayName = "It should update the quantity into the inventory.")]
  [InlineData(false, false)]
  [InlineData(true, false)]
  [InlineData(false, true)]
  [InlineData(true, true)]
  public async Task Given_Payload_When_Update_Then_QuantityUpdated(bool exists, bool removed)
  {
    if (exists)
    {
      InventoryAggregate inventory = new(_trainer);
      inventory.Add(_item, Faker.Random.Int(1, 10), World.OwnerId);
      await _inventoryRepository.SaveAsync(inventory);
    }

    InventoryQuantityPayload payload = new(removed ? 0 : Faker.Random.Int(1, 10));
    InventoryItemModel result = await _inventoryService.UpdateAsync(_trainer.EntityId, _item.EntityId, payload);

    Assert.Equal(_item.EntityId, result.Item.Id);
    Assert.Equal(payload.Quantity, result.Quantity);
  }
}
