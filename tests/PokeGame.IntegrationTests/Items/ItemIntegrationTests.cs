using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class ItemIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;

  public ItemIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = new ItemBuilder(Faker).WithWorld(World).Build();
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should read an item by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _item.EntityId;
    ItemModel? item = await _itemService.ReadAsync(id);
    Assert.NotNull(item);
    Assert.Equal(id, item.Id);
  }

  [Fact(DisplayName = "It should read an item by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    string key = $"  {_item.Key.Value.ToUpperInvariant()}  ";
    ItemModel? item = await _itemService.ReadAsync(id: null, key);
    Assert.NotNull(item);
    Assert.Equal(_item.EntityId, item.Id);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Assert.Fail("TODO(fpion): implement");
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = _item.Key.Value.ToUpperInvariant(),
      OtherItem = new OtherItemPropertiesModel()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _itemService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Item", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_item.EntityId, exception.ConflictId);
    Assert.Equal(_item.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }
}
