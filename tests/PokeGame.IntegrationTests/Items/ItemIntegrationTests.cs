using Krakenar.Contracts.Search;
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

  [Fact(DisplayName = "It should create a new item with ID.")]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "ability-capsule",
      Name = " Ability Capsule ",
      Description = "  A capsule that allows a Pokémon to switch its current Ability to the other Ability its species can have.  ",
      Price = 100000,
      Sprite = "https://archives.bulbagarden.net/media/upload/7/77/Dream_Ability_Capsule_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ability_Capsule",
      Notes = "   Consumable item that swaps a Pokémon’s standard Ability to its other one (if available); cannot grant Hidden Abilities and works only outside battle.   ",
      OtherItem = new OtherItemPropertiesModel()
    };
    Guid id = Guid.NewGuid();

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(id, item.Id);
    Assert.Equal(3, item.Version);
    Assert.Equal(Actor, item.CreatedBy);
    Assert.Equal(DateTime.UtcNow, item.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.OtherItem, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.OtherItem, item.OtherItem);
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
    Item abilityCapsule = new ItemBuilder(Faker).WithWorld(World).WithKey(new Slug("ability-capsule")).Build();
    Item abilityPatch = new ItemBuilder(Faker).WithWorld(World).WithKey(new Slug("ability-patch")).Build();
    Item abilityShield = new ItemBuilder(Faker).WithWorld(World).WithKey(new Slug("ability-shield")).Build();
    Item nugget = new ItemBuilder(Faker).WithWorld(World).WithKey(new Slug("nugget")).IsTreasure().Build();
    await _itemRepository.SaveAsync([abilityCapsule, abilityPatch, abilityShield, nugget]);

    SearchItemsPayload payload = new()
    {
      Category = ItemCategory.OtherItem,
      Ids = [_item.EntityId, abilityCapsule.EntityId, abilityPatch.EntityId, Guid.Empty, nugget.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("ability%"));
    payload.Search.Terms.Add(new SearchTerm("%gg%"));
    payload.Sort.Add(new ItemSortOption(ItemSort.Key, isDescending: true));

    SearchResults<ItemModel> results = await _itemService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    ItemModel item = Assert.Single(results.Items);
    Assert.Equal(abilityCapsule.EntityId, item.Id);
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
