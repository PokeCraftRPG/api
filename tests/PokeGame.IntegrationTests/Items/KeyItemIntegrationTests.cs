using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class KeyItemIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;

  public KeyItemIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = new ItemBuilder(Faker).WithWorld(World).IsKeyItem().Build();
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should create a new key item.")]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "old-rod",
      Name = " Old Rod ",
      Description = "  An old and beat-up fishing rod. Use it at bodies of water to fish for Pokémon.  ",
      Sprite = "https://archives.bulbagarden.net/media/upload/d/d5/RG_Old_Rod.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Old_Rod",
      Notes = "   A basic fishing tool used to catch low-level Pokémon in water; weaker than better rods but widely available early in the game.   ",
      KeyItem = new KeyItemPropertiesModel()
    };

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id: null);
    Assert.True(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(3, item.Version);
    Assert.Equal(Actor, item.CreatedBy);
    Assert.Equal(DateTime.UtcNow, item.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.KeyItem, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.KeyItem, item.KeyItem);
  }

  [Fact(DisplayName = "It should replace an existing key item.")]
  public async Task Given_DoesExist_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "old-rod",
      Name = " Old Rod ",
      Description = "  An old and beat-up fishing rod. Use it at bodies of water to fish for Pokémon.  ",
      Sprite = "https://archives.bulbagarden.net/media/upload/d/d5/RG_Old_Rod.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Old_Rod",
      Notes = "   A basic fishing tool used to catch low-level Pokémon in water; weaker than better rods but widely available early in the game.   ",
      KeyItem = new KeyItemPropertiesModel()
    };

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, _item.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 2, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.KeyItem, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.KeyItem, item.KeyItem);
  }

  [Fact(DisplayName = "It should update an existing key item.")]
  public async Task Given_DoesExist_When_Update_Then_Updated()
  {
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(" Old Rod "),
      Description = new Optional<string>("  An old and beat-up fishing rod. Use it at bodies of water to fish for Pokémon.  "),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/d/d5/RG_Old_Rod.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Old_Rod"),
      Notes = new Optional<string>("   A basic fishing tool used to catch low-level Pokémon in water; weaker than better rods but widely available early in the game.   "),
      KeyItem = new KeyItemPropertiesModel()
    };

    ItemModel? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);

    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 1, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.KeyItem, item.Category);
    Assert.Equal(_item.Key.Value, item.Key);
    Assert.Equal(payload.Name.Value?.Trim(), item.Name);
    Assert.Equal(payload.Description.Value?.Trim(), item.Description);
    Assert.Null(item.Price);
    Assert.Equal(payload.Sprite.Value, item.Sprite);
    Assert.Equal(payload.Url.Value, item.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), item.Notes);
    Assert.Equal(payload.KeyItem, item.KeyItem);
  }
}
