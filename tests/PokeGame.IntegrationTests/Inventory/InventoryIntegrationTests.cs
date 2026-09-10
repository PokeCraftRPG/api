using FluentValidation;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Trainers;

namespace PokeGame.Inventory;

[Trait(Traits.Category, Categories.Integration)]
public class InventoryIntegrationTests : IntegrationTests
{
  private readonly IInventoryService _inventoryService;
  private readonly IItemRepository _itemRepository;
  private readonly ITrainerRepository _trainerRepository;

  private Trainer _trainer = null!;
  private Item _item = null!;

  public InventoryIntegrationTests()
  {
    _inventoryService = ServiceProvider.GetRequiredService<IInventoryService>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    _item = ItemBuilder.Potion(Faker, Context.World);
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should add an item to the inventory.")]
  public async Task Given_NotInInventory_When_Set_Then_Added()
  {
    SetInventoryItemPayload payload = new()
    {
      Quantity = 5
    };

    InventoryItemDto inventoryItem = await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, payload);
    AssertInventoryItem(_item, payload.Quantity, inventoryItem);

    InventoryItemDto? read = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(read);
    AssertInventoryItem(_item, payload.Quantity, read);
  }

  [Fact(DisplayName = "It should update an existing inventory quantity.")]
  public async Task Given_Exists_When_Set_Then_Updated()
  {
    await SetAsync(3);

    SetInventoryItemPayload payload = new()
    {
      Quantity = 12
    };

    InventoryItemDto inventoryItem = await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, payload);
    AssertInventoryItem(_item, payload.Quantity, inventoryItem);

    InventoryItemDto? read = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(read);
    Assert.Equal(payload.Quantity, read.Quantity);
  }

  [Fact(DisplayName = "It should remove an item when the quantity is set to 0.")]
  public async Task Given_Exists_When_SetZero_Then_Removed()
  {
    await SetAsync(8);

    InventoryItemDto inventoryItem = await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, new SetInventoryItemPayload());
    AssertInventoryItem(_item, 0, inventoryItem);

    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));

    SearchResults<InventoryItemDto>? results = await SearchAsync();
    Assert.NotNull(results);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return quantity 0 when setting 0 for an item that is not in the inventory.")]
  public async Task Given_NotInInventory_When_SetZero_Then_QuantityZero()
  {
    InventoryItemDto inventoryItem = await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, new SetInventoryItemPayload());
    AssertInventoryItem(_item, 0, inventoryItem);

    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));
  }

  [Fact(DisplayName = "It should keep the quantity unchanged when setting the same value.")]
  public async Task Given_SameQuantity_When_Set_Then_Unchanged()
  {
    await SetAsync(7);

    InventoryItemDto inventoryItem = await SetAsync(7);
    AssertInventoryItem(_item, 7, inventoryItem);

    InventoryItemDto? read = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(read);
    Assert.Equal(7, read.Quantity);
  }

  [Fact(DisplayName = "It should set the maximum inventory quantity.")]
  public async Task Given_MaximumQuantity_When_Set_Then_Set()
  {
    InventoryItemDto inventoryItem = await SetAsync(TrainerInventory.MaximumQuantity);
    AssertInventoryItem(_item, TrainerInventory.MaximumQuantity, inventoryItem);
  }

  [Fact(DisplayName = "It should add an item when adjusting a positive delta.")]
  public async Task Given_NotInInventory_When_AdjustPositive_Then_Added()
  {
    AdjustInventoryItemPayload payload = new()
    {
      Delta = 4
    };

    InventoryItemDto inventoryItem = await _inventoryService.AdjustAsync(_trainer.EntityId, _item.EntityId, payload);
    AssertInventoryItem(_item, payload.Delta, inventoryItem);

    InventoryItemDto? read = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(read);
    Assert.Equal(payload.Delta, read.Quantity);
  }

  [Fact(DisplayName = "It should increase an existing inventory quantity.")]
  public async Task Given_Exists_When_AdjustPositive_Then_Increased()
  {
    await SetAsync(10);

    InventoryItemDto inventoryItem = await AdjustAsync(5);
    AssertInventoryItem(_item, 15, inventoryItem);
  }

  [Fact(DisplayName = "It should decrease an existing inventory quantity.")]
  public async Task Given_Exists_When_AdjustNegative_Then_Decreased()
  {
    await SetAsync(10);

    InventoryItemDto inventoryItem = await AdjustAsync(-3);
    AssertInventoryItem(_item, 7, inventoryItem);
  }

  [Fact(DisplayName = "It should remove an item when adjusting the quantity to 0.")]
  public async Task Given_Exists_When_AdjustToZero_Then_Removed()
  {
    await SetAsync(6);

    InventoryItemDto inventoryItem = await AdjustAsync(-6);
    AssertInventoryItem(_item, 0, inventoryItem);

    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when adjusting above the maximum.")]
  public async Task Given_WouldExceedMaximum_When_Adjust_Then_InventoryQuantityOutOfRangeException()
  {
    await SetAsync(TrainerInventory.MaximumQuantity);

    InventoryQuantityOutOfRangeException exception = await Assert.ThrowsAsync<InventoryQuantityOutOfRangeException>(
      async () => await AdjustAsync(1));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.Equal(_item.EntityId, exception.ItemId);
    Assert.Equal(TrainerInventory.MinimumQuantity, exception.MinimumQuantity);
    Assert.Equal(TrainerInventory.MaximumQuantity, exception.MaximumQuantity);
    Assert.Equal(TrainerInventory.MaximumQuantity + 1, exception.AttemptedQuantity);
    Assert.Equal("Quantity", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when adjusting below the minimum.")]
  public async Task Given_WouldGoBelowMinimum_When_Adjust_Then_InventoryQuantityOutOfRangeException()
  {
    InventoryQuantityOutOfRangeException exception = await Assert.ThrowsAsync<InventoryQuantityOutOfRangeException>(
      async () => await AdjustAsync(-1));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.Equal(_item.EntityId, exception.ItemId);
    Assert.Equal(TrainerInventory.MinimumQuantity, exception.MinimumQuantity);
    Assert.Equal(TrainerInventory.MaximumQuantity, exception.MaximumQuantity);
    Assert.Equal(-1, exception.AttemptedQuantity);
    Assert.Equal("Quantity", exception.PropertyName);
  }

  [Fact(DisplayName = "It should read an inventory item.")]
  public async Task Given_Exists_When_Read_Then_Read()
  {
    await SetAsync(9);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId);
    Assert.NotNull(inventoryItem);
    AssertInventoryItem(_item, 9, inventoryItem);
  }

  [Fact(DisplayName = "It should return null when the item is not in the inventory.")]
  public async Task Given_NotInInventory_When_Read_Then_NullReturned()
  {
    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));
  }

  [Fact(DisplayName = "It should return null when reading from another world.")]
  public async Task Given_WrongWorld_When_Read_Then_NullReturned()
  {
    await SetAsync(2);

    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _item.EntityId));
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    await SetAsync(1);

    SearchInventoryItemsPayload payload = new()
    {
      Limit = 10
    };
    payload.Search.Terms.Add("zzzzz");

    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(_trainer.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return empty search results when the inventory is empty.")]
  public async Task Given_EmptyInventory_When_Search_Then_EmptyResults()
  {
    SearchResults<InventoryItemDto>? results = await SearchAsync();
    Assert.NotNull(results);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when searching a missing trainer.")]
  public async Task Given_MissingTrainer_When_Search_Then_NullReturned()
  {
    Assert.Null(await SearchAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    Item antidote = ItemBuilder.Antidote(Faker, Context.World);
    Item masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync([superPotion, antidote, masterBall]);

    await SetAsync(10);
    await SetAsync(5, superPotion);
    await SetAsync(20, antidote);
    await SetAsync(1, masterBall);

    SearchInventoryItemsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("super");
    payload.Search.Terms.Add("antidote");
    payload.Ids.AddRange([superPotion.EntityId, antidote.EntityId]);
    payload.Sort.Add(new SortOption<InventoryItemSort>(InventoryItemSort.Name, SortDirection.Descending));

    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(_trainer.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(2, results.Total);

    InventoryItemDto inventoryItem = Assert.Single(results.Items);
    Assert.Equal(antidote.EntityId, inventoryItem.Item.Id);
    Assert.Equal(20, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should filter search results by category.")]
  public async Task Given_Category_When_Search_Then_Filtered()
  {
    Item masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync(masterBall);

    await SetAsync(3);
    await SetAsync(1, masterBall);

    SearchInventoryItemsPayload payload = new()
    {
      Category = ItemCategory.PokeBall,
      Limit = 10
    };

    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(_trainer.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(1, results.Total);

    InventoryItemDto inventoryItem = Assert.Single(results.Items);
    Assert.Equal(masterBall.EntityId, inventoryItem.Item.Id);
    Assert.Equal(ItemCategory.PokeBall, inventoryItem.Item.Category);
    Assert.Equal(1, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should sort search results by quantity.")]
  public async Task Given_QuantitySort_When_Search_Then_Ordered()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    Item antidote = ItemBuilder.Antidote(Faker, Context.World);
    Item masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync([superPotion, antidote, masterBall]);

    await SetAsync(10);
    await SetAsync(5, superPotion);
    await SetAsync(20, antidote);
    await SetAsync(1, masterBall);

    SearchInventoryItemsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Sort.Add(new SortOption<InventoryItemSort>(InventoryItemSort.Quantity, SortDirection.Descending));

    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(_trainer.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(4, results.Total);

    InventoryItemDto inventoryItem = Assert.Single(results.Items);
    Assert.Equal(_item.EntityId, inventoryItem.Item.Id);
    Assert.Equal(10, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should return the total without items when the search limit is 0.")]
  public async Task Given_ZeroLimit_When_Search_Then_TotalOnly()
  {
    await SetAsync(4);

    SearchInventoryItemsPayload payload = new()
    {
      Limit = 0
    };

    SearchResults<InventoryItemDto>? results = await _inventoryService.SearchAsync(_trainer.EntityId, payload);
    Assert.NotNull(results);
    Assert.Equal(1, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should not include another trainer's items in search results.")]
  public async Task Given_OtherTrainer_When_Search_Then_NotIncluded()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    await _itemRepository.SaveAsync(superPotion);

    await SetAsync(10);
    await _inventoryService.SetAsync(blue.EntityId, superPotion.EntityId, new SetInventoryItemPayload { Quantity = 99 });

    SearchResults<InventoryItemDto>? results = await SearchAsync();
    Assert.NotNull(results);
    Assert.Equal(1, results.Total);

    InventoryItemDto inventoryItem = Assert.Single(results.Items);
    Assert.Equal(_item.EntityId, inventoryItem.Item.Id);
    Assert.Equal(10, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should throw ValidationException when the search payload is invalid.")]
  public async Task Given_InvalidPayload_When_Search_Then_ValidationException()
  {
    SearchInventoryItemsPayload payload = new()
    {
      Limit = -1
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _inventoryService.SearchAsync(_trainer.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the trainer does not exist.")]
  public async Task Given_MissingTrainer_When_Set_Then_EntityNotFoundException()
  {
    Guid missingTrainerId = Guid.NewGuid();
    SetInventoryItemPayload payload = new()
    {
      Quantity = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _inventoryService.SetAsync(missingTrainerId, _item.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Trainer.EntityKind, exception.EntityKind);
    Assert.Equal(missingTrainerId, exception.EntityId);
    Assert.Equal("TrainerId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the item does not exist.")]
  public async Task Given_MissingItem_When_Set_Then_EntityNotFoundException()
  {
    Guid missingItemId = Guid.NewGuid();
    SetInventoryItemPayload payload = new()
    {
      Quantity = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _inventoryService.SetAsync(_trainer.EntityId, missingItemId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(missingItemId, exception.EntityId);
    Assert.Equal("ItemId", exception.PropertyName);
  }

  [Theory(DisplayName = "It should throw ValidationException when the set payload is invalid.")]
  [InlineData(-1)]
  [InlineData(1000)]
  public async Task Given_InvalidPayload_When_Set_Then_ValidationException(int quantity)
  {
    SetInventoryItemPayload payload = new()
    {
      Quantity = quantity
    };

    await Assert.ThrowsAsync<ValidationException>(
      async () => await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when setting an inventory item.")]
  public async Task Given_NotAllowed_When_Set_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    SetInventoryItemPayload payload = new()
    {
      Quantity = 1
    };

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _inventoryService.SetAsync(_trainer.EntityId, _item.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(new InventoryId(_trainer.Id).GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when adjusting for a missing trainer.")]
  public async Task Given_MissingTrainer_When_Adjust_Then_EntityNotFoundException()
  {
    Guid missingTrainerId = Guid.NewGuid();
    AdjustInventoryItemPayload payload = new()
    {
      Delta = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _inventoryService.AdjustAsync(missingTrainerId, _item.EntityId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Trainer.EntityKind, exception.EntityKind);
    Assert.Equal(missingTrainerId, exception.EntityId);
    Assert.Equal("TrainerId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when adjusting a missing item.")]
  public async Task Given_MissingItem_When_Adjust_Then_EntityNotFoundException()
  {
    Guid missingItemId = Guid.NewGuid();
    AdjustInventoryItemPayload payload = new()
    {
      Delta = 1
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _inventoryService.AdjustAsync(_trainer.EntityId, missingItemId, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(missingItemId, exception.EntityId);
    Assert.Equal("ItemId", exception.PropertyName);
  }

  [Theory(DisplayName = "It should throw ValidationException when the adjust payload is invalid.")]
  [InlineData(0)]
  [InlineData(1000)]
  [InlineData(-1000)]
  public async Task Given_InvalidPayload_When_Adjust_Then_ValidationException(int delta)
  {
    AdjustInventoryItemPayload payload = new()
    {
      Delta = delta
    };

    await Assert.ThrowsAsync<ValidationException>(
      async () => await _inventoryService.AdjustAsync(_trainer.EntityId, _item.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when adjusting an inventory item.")]
  public async Task Given_NotAllowed_When_Adjust_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    AdjustInventoryItemPayload payload = new()
    {
      Delta = 1
    };

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _inventoryService.AdjustAsync(_trainer.EntityId, _item.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(new InventoryId(_trainer.Id).GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  private Task<InventoryItemDto> SetAsync(int quantity, Item? item = null)
  {
    SetInventoryItemPayload payload = new()
    {
      Quantity = quantity
    };
    return _inventoryService.SetAsync(_trainer.EntityId, (item ?? _item).EntityId, payload);
  }

  private Task<InventoryItemDto> AdjustAsync(int delta, Item? item = null)
  {
    AdjustInventoryItemPayload payload = new()
    {
      Delta = delta
    };
    return _inventoryService.AdjustAsync(_trainer.EntityId, (item ?? _item).EntityId, payload);
  }

  private Task<SearchResults<InventoryItemDto>?> SearchAsync(Guid? trainerId = null)
  {
    SearchInventoryItemsPayload payload = new()
    {
      Limit = 10
    };
    return _inventoryService.SearchAsync(trainerId ?? _trainer.EntityId, payload);
  }

  private static void AssertInventoryItem(Item expected, int quantity, InventoryItemDto actual)
  {
    AssertItem(expected, actual.Item);
    Assert.Equal(quantity, actual.Quantity);
  }

  private static void AssertItem(Item expected, ItemDto actual)
  {
    Assert.Equal(expected.EntityId, actual.Id);
    Assert.Equal(expected.Category, actual.Category);
    Assert.Equal(expected.Key.Value, actual.Key);
    Assert.Equal(expected.Name?.Value, actual.Name);
  }
}
