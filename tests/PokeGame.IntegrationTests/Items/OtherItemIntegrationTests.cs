using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class OtherItemIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;

  public OtherItemIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = new ItemBuilder(Faker).WithWorld(World).Build(); // TODO(fpion): Category/Properties
    await _itemRepository.SaveAsync(_item);
  }

  [Theory(DisplayName = "It should create a new other item.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

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

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, item.Id);
    }
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

  [Fact(DisplayName = "It should replace an existing other item.")]
  public async Task Given_DoesExist_When_CreateOrReplace_Then_Replaced()
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

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, _item.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 2, item.Version);
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

  [Fact(DisplayName = "It should update an existing other item.")]
  public async Task Given_DoesExist_When_Update_Then_Updated()
  {
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(" Ability Capsule "),
      Description = new Optional<string>("  A capsule that allows a Pokémon to switch its current Ability to the other Ability its species can have.  "),
      Price = new Optional<int?>(100000),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/7/77/Dream_Ability_Capsule_Sprite.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Ability_Capsule"),
      Notes = new Optional<string>("   Consumable item that swaps a Pokémon’s standard Ability to its other one (if available); cannot grant Hidden Abilities and works only outside battle.   "),
      OtherItem = new OtherItemPropertiesModel()
    };

    ItemModel? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);

    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 1, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.OtherItem, item.Category);
    Assert.Equal(_item.Key.Value, item.Key);
    Assert.Equal(payload.Name.Value?.Trim(), item.Name);
    Assert.Equal(payload.Description.Value?.Trim(), item.Description);
    Assert.Equal(payload.Price.Value, item.Price);
    Assert.Equal(payload.Sprite.Value, item.Sprite);
    Assert.Equal(payload.Url.Value, item.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), item.Notes);
    Assert.Equal(payload.OtherItem, item.OtherItem);
  }
}
