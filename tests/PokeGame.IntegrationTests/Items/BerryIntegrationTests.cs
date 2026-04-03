using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class BerryIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;

  public BerryIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = new ItemBuilder(Faker).WithWorld(World).IsBerry().Build();
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should create a new berry item.")]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "oran-berry",
      Name = " Oran Berry ",
      Description = "  A berry that can be used to oran-berry a Pokémon that has fainted. It also restores half the Pokémon's max HP.  ",
      Price = 80,
      Sprite = "https://archives.bulbagarden.net/media/upload/0/0c/Dream_Oran_Berry_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Oran_Berry",
      Notes = "   A common Berry that restores 10 HP (or more in some games); auto-consumed in battle or used from the Bag, and widely found or grown.   ",
      Berry = new BerryPropertiesModel(healing: 10)
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

    Assert.Equal(ItemCategory.Berry, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.Berry, item.Berry);
  }

  [Fact(DisplayName = "It should replace an existing berry item.")]
  public async Task Given_DoesExist_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "oran-berry",
      Name = " Oran Berry ",
      Description = "  A berry that can be used to oran-berry a Pokémon that has fainted. It also restores half the Pokémon's max HP.  ",
      Price = 80,
      Sprite = "https://archives.bulbagarden.net/media/upload/0/0c/Dream_Oran_Berry_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Oran_Berry",
      Notes = "   A common Berry that restores 10 HP (or more in some games); auto-consumed in battle or used from the Bag, and widely found or grown.   ",
      Berry = new BerryPropertiesModel(healing: 10)
    };

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, _item.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 3, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.Berry, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.Berry, item.Berry);
  }

  [Fact(DisplayName = "It should update an existing berry item.")]
  public async Task Given_DoesExist_When_Update_Then_Updated()
  {
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(" Oran Berry "),
      Description = new Optional<string>("  A berry that can be used to oran-berry a Pokémon that has fainted. It also restores half the Pokémon's max HP.  "),
      Price = new Optional<int?>(80),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/0/0c/Dream_Oran_Berry_Sprite.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Oran_Berry"),
      Notes = new Optional<string>("   A common Berry that restores 10 HP (or more in some games); auto-consumed in battle or used from the Bag, and widely found or grown.   "),
      Berry = new BerryPropertiesModel(healing: 10)
    };

    ItemModel? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);

    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 2, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.Berry, item.Category);
    Assert.Equal(_item.Key.Value, item.Key);
    Assert.Equal(payload.Name.Value?.Trim(), item.Name);
    Assert.Equal(payload.Description.Value?.Trim(), item.Description);
    Assert.Equal(payload.Price.Value, item.Price);
    Assert.Equal(payload.Sprite.Value, item.Sprite);
    Assert.Equal(payload.Url.Value, item.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), item.Notes);
    Assert.Equal(payload.Berry, item.Berry);
  }
}
