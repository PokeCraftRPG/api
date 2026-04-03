using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class BattleItemIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;

  public BattleItemIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = new ItemBuilder(Faker).WithWorld(World).IsBattleItem().Build();
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should create a new battle item.")]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "guard-spec",
      Name = " Guard Spec. ",
      Description = "  An item that prevents Pokémon on your side from having their stats lowered in battle for five turns.  ",
      Price = 1500,
      Sprite = "https://archives.bulbagarden.net/media/upload/e/e0/Dream_Guard_Spec._Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Guard_Spec.",
      Notes = "   A battle item that prevents stat reductions for 5 turns (Mist effect), protecting your team from enemy debuffs but not self-inflicted drops.   ",
      BattleItem = new BattleItemPropertiesModel(guardTurns: 5)
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

    Assert.Equal(ItemCategory.BattleItem, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.BattleItem, item.BattleItem);
  }

  [Fact(DisplayName = "It should replace an existing battle item.")]
  public async Task Given_DoesExist_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "guard-spec",
      Name = " Guard Spec. ",
      Description = "  An item that prevents Pokémon on your side from having their stats lowered in battle for five turns.  ",
      Price = 1500,
      Sprite = "https://archives.bulbagarden.net/media/upload/e/e0/Dream_Guard_Spec._Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Guard_Spec.",
      Notes = "   A battle item that prevents stat reductions for 5 turns (Mist effect), protecting your team from enemy debuffs but not self-inflicted drops.   ",
      BattleItem = new BattleItemPropertiesModel(guardTurns: 5)
    };

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, _item.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 3, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.BattleItem, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(payload.BattleItem, item.BattleItem);
  }

  [Fact(DisplayName = "It should update an existing battle item.")]
  public async Task Given_DoesExist_When_Update_Then_Updated()
  {
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(" Guard Spec. "),
      Description = new Optional<string>("  An item that prevents Pokémon on your side from having their stats lowered in battle for five turns.  "),
      Price = new Optional<int?>(1500),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/e/e0/Dream_Guard_Spec._Sprite.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Guard_Spec."),
      Notes = new Optional<string>("   A battle item that prevents stat reductions for 5 turns (Mist effect), protecting your team from enemy debuffs but not self-inflicted drops.   "),
      BattleItem = new BattleItemPropertiesModel(guardTurns: 5)
    };

    ItemModel? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);

    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 2, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.BattleItem, item.Category);
    Assert.Equal(_item.Key.Value, item.Key);
    Assert.Equal(payload.Name.Value?.Trim(), item.Name);
    Assert.Equal(payload.Description.Value?.Trim(), item.Description);
    Assert.Equal(payload.Price.Value, item.Price);
    Assert.Equal(payload.Sprite.Value, item.Sprite);
    Assert.Equal(payload.Url.Value, item.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), item.Notes);
    Assert.Equal(payload.BattleItem, item.BattleItem);
  }
}
