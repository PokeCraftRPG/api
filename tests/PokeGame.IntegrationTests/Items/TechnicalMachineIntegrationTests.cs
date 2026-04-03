using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Moves;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class TechnicalMachineIntegrationTests : IntegrationTests
{
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;
  private readonly IMoveRepository _moveRepository;

  private Item _item = null!;
  private Move _thunderPunch = null!;
  private Move _thunderShock = null!;

  public TechnicalMachineIntegrationTests()
  {
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _thunderPunch = MoveBuilder.ThunderPunch(Faker, World);
    _thunderShock = MoveBuilder.ThunderShock(Faker, World);
    await _moveRepository.SaveAsync([_thunderPunch, _thunderShock]);

    _item = new ItemBuilder(Faker).WithWorld(World).IsTechnicalMachine(new TechnicalMachineProperties(_thunderShock)).Build();
    await _itemRepository.SaveAsync(_item);
  }

  [Fact(DisplayName = "It should create a new TM item.")]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "tm-999",
      Name = " TM 999 - Thunder Punch ",
      Description = "  The target is attacked with an electrified punch. This may also leave the target with paralysis.  ",
      Sprite = "https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/TM",
      Notes = "   An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.   ",
      TechnicalMachine = new TechnicalMachinePropertiesPayload($"    {_thunderPunch.EntityId.ToString().ToUpperInvariant()}    ")
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

    Assert.Equal(ItemCategory.TechnicalMachine, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(_thunderPunch.EntityId, item.TechnicalMachine?.Move.Id);
  }

  [Fact(DisplayName = "It should replace an existing TM item.")]
  public async Task Given_DoesExist_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "tm-999",
      Name = " TM 999 - Thunder Punch ",
      Description = "  The target is attacked with an electrified punch. This may also leave the target with paralysis.  ",
      Sprite = "https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/TM",
      Notes = "   An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.   ",
      TechnicalMachine = new TechnicalMachinePropertiesPayload($"    {_thunderPunch.Key.Value.ToUpperInvariant()}    ")
    };

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, _item.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Item);

    ItemModel item = result.Item;
    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 3, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.TechnicalMachine, item.Category);
    Assert.Equal(payload.Key, item.Key);
    Assert.Equal(payload.Name.Trim(), item.Name);
    Assert.Equal(payload.Description.Trim(), item.Description);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Sprite, item.Sprite);
    Assert.Equal(payload.Url, item.Url);
    Assert.Equal(payload.Notes.Trim(), item.Notes);
    Assert.Equal(_thunderPunch.EntityId, item.TechnicalMachine?.Move.Id);
  }

  [Fact(DisplayName = "It should update an existing TM item.")]
  public async Task Given_DoesExist_When_Update_Then_Updated()
  {
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(" TM 999 - Thunder Punch "),
      Description = new Optional<string>("  The target is attacked with an electrified punch. This may also leave the target with paralysis.  "),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/TM"),
      Notes = new Optional<string>("   An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.   ")
    };

    ItemModel? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);

    Assert.Equal(_item.EntityId, item.Id);
    Assert.Equal(_item.Version + 1, item.Version);
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(ItemCategory.TechnicalMachine, item.Category);
    Assert.Equal(_item.Key.Value, item.Key);
    Assert.Equal(payload.Name.Value?.Trim(), item.Name);
    Assert.Equal(payload.Description.Value?.Trim(), item.Description);
    Assert.Null(item.Price);
    Assert.Equal(payload.Sprite.Value, item.Sprite);
    Assert.Equal(payload.Url.Value, item.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), item.Notes);
    Assert.Equal(_thunderShock.EntityId, item.TechnicalMachine?.Move.Id);
  }
}
