using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;

namespace PokeGame.Items;

[Trait(Traits.Category, Categories.Integration)]
public class ItemIntegrationTests : IntegrationTests
{
  private readonly IAssetService _assetService;
  private readonly IItemRepository _itemRepository;
  private readonly IItemService _itemService;

  private Item _item = null!;
  private ItemDto _seeded = null!;

  public ItemIntegrationTests()
  {
    _assetService = ServiceProvider.GetRequiredService<IAssetService>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _itemService = ServiceProvider.GetRequiredService<IItemService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _item = ItemBuilder.Potion(Faker, Context.World);
    await _itemRepository.SaveAsync(_item);

    _seeded = (await _itemService.ReadAsync(_item.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new item.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceItemPayload payload = CreateSuperPotionPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    ItemDto item = result.Item;
    Assert.NotNull(item);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, item.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, item.Id);
    }
    Assert.Equal(3, item.Version);
    Assert.Equal(Actor, item.CreatedBy);
    Assert.Equal(DateTime.UtcNow, item.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(item.CreatedBy, item.UpdatedBy);
    Assert.True(item.CreatedOn < item.UpdatedOn);

    AssertSuperPotion(payload, item);
  }

  [Fact(DisplayName = "It should create an item with a sprite.")]
  public async Task Given_Sprite_When_Create_Then_Created()
  {
    AssetDto sprite = await UploadSpriteAsync();
    CreateOrReplaceItemPayload payload = CreateSuperPotionPayload();
    payload.SpriteId = sprite.Id;

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload);
    Assert.True(result.Created);
    ItemDto item = result.Item;

    AssertSuperPotion(payload, item);
    Assert.NotNull(item.Sprite);
    Assert.Equal(sprite.Id, item.Sprite.Id);
  }

  [Fact(DisplayName = "It should read an item by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    ItemDto? item = await _itemService.ReadAsync(_item.EntityId);
    Assert.NotNull(item);
    Assert.Equal(_item.EntityId, item.Id);
  }

  [Fact(DisplayName = "It should read an item by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    ItemDto? item = await _itemService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(item);
    Assert.Equal(_item.EntityId, item.Id);
  }

  [Fact(DisplayName = "It should replace an existing item.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceItemPayload payload = CreateUpdatedPotionPayload();
    payload.Key = _seeded.Key;
    Guid id = _item.EntityId;

    CreateOrReplaceItemResult result = await _itemService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    ItemDto item = result.Item;
    Assert.NotNull(item);

    Assert.Equal(id, item.Id);
    Assert.Equal(5, item.Version);
    Assert.Equal(_seeded.CreatedBy, item.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, item.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedPotion(payload, item);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchItemsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<ItemDto> results = await _itemService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no item was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _itemService.ReadAsync(_item.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many items were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    await _itemRepository.SaveAsync(superPotion);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _itemService.ReadAsync(_item.EntityId, superPotion.Key.Value));
    TooManyResultsException<ItemDto> tooMany = Assert.IsType<TooManyResultsException<ItemDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the item was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _itemService.UpdateAsync(Guid.Empty, new UpdateItemPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    Item antidote = ItemBuilder.Antidote(Faker, Context.World);
    Item masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync([superPotion, antidote, masterBall]);

    SearchItemsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("super");
    payload.Search.Terms.Add("antidote");
    payload.Ids.AddRange([superPotion.EntityId, antidote.EntityId]);
    payload.Sort.Add(new SortOption<ItemSort>(ItemSort.Name, SortDirection.Descending));

    SearchResults<ItemDto> results = await _itemService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    ItemDto item = Assert.Single(results.Items);
    Assert.Equal(antidote.EntityId, item.Id);
  }

  [Fact(DisplayName = "It should filter search results by category.")]
  public async Task Given_Category_When_Search_Then_Filtered()
  {
    Item masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync(masterBall);

    SearchItemsPayload payload = new()
    {
      Category = ItemCategory.PokeBall,
      Limit = 10
    };

    SearchResults<ItemDto> results = await _itemService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    ItemDto item = Assert.Single(results.Items);
    Assert.Equal(masterBall.EntityId, item.Id);
    Assert.Equal(ItemCategory.PokeBall, item.Category);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating an item and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Category = ItemCategory.Medicine,
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _itemService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_item.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Item.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing an item and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    await _itemRepository.SaveAsync(superPotion);

    CreateOrReplaceItemPayload payload = new()
    {
      Category = ItemCategory.Medicine,
      Key = _seeded.Key
    };
    Guid id = superPotion.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _itemService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_item.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Item.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an item and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Item superPotion = ItemBuilder.SuperPotion(Faker, Context.World);
    await _itemRepository.SaveAsync(superPotion);

    UpdateItemPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = superPotion.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _itemService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_item.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Item.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing an item with a different category.")]
  public async Task Given_DifferentCategory_When_Replace_Then_ImmutablePropertyException()
  {
    CreateOrReplaceItemPayload payload = CreateUpdatedPotionPayload();
    payload.Category = ItemCategory.Battle;

    ImmutablePropertyException<ItemCategory> exception = await Assert.ThrowsAsync<ImmutablePropertyException<ItemCategory>>(
      async () => await _itemService.CreateOrReplaceAsync(payload, _item.EntityId));
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(_item.EntityId, exception.EntityId);
    Assert.Equal(_seeded.Category, exception.ExpectedValue);
    Assert.Equal(payload.Category, exception.AttemptedValue);
    Assert.Equal(nameof(payload.Category), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Category = ItemCategory.Medicine,
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _itemService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateItemPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _itemService.UpdateAsync(_item.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw InvalidAssetKindException when the sprite is not an image.")]
  public async Task Given_VideoSprite_When_Create_Then_InvalidAssetKindException()
  {
    AssetDto video = await UploadVideoAsync();
    CreateOrReplaceItemPayload payload = CreateSuperPotionPayload();
    payload.SpriteId = video.Id;

    InvalidAssetKindException exception = await Assert.ThrowsAsync<InvalidAssetKindException>(
      async () => await _itemService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(video.Id, exception.AssetId);
    Assert.Equal(AssetKind.Image, exception.ExpectedKind);
    Assert.Equal(AssetKind.Video, exception.AttemptedKind);
    Assert.Equal(nameof(Item.SpriteId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating an item.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceItemPayload payload = CreateSuperPotionPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _itemService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateItem", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an item.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    CreateOrReplaceItemPayload payload = CreateUpdatedPotionPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _itemService.CreateOrReplaceAsync(payload, _item.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_item.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an item.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    UpdateItemPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _itemService.UpdateAsync(_item.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_item.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing item.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _item.EntityId;
    CreateOrReplaceItemPayload create = CreateUpdatedPotionPayload();
    UpdateItemPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      Price = new Optional<int?>(create.Price),
      Weight = new Optional<int?>(create.Weight)
    };

    ItemDto? item = await _itemService.UpdateAsync(id, payload);
    Assert.NotNull(item);

    Assert.Equal(id, item.Id);
    Assert.Equal(5, item.Version);
    Assert.Equal(_seeded.CreatedBy, item.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, item.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, item.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, item.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedPotion(create, item);
  }

  [Fact(DisplayName = "It should update an item sprite.")]
  public async Task Given_Sprite_When_Update_Then_Updated()
  {
    AssetDto sprite = await UploadSpriteAsync();
    UpdateItemPayload payload = new()
    {
      SpriteId = new Optional<Guid?>(sprite.Id)
    };

    ItemDto? item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);
    Assert.NotNull(item.Sprite);
    Assert.Equal(sprite.Id, item.Sprite.Id);

    payload = new()
    {
      SpriteId = new Optional<Guid?>(null)
    };
    item = await _itemService.UpdateAsync(_item.EntityId, payload);
    Assert.NotNull(item);
    Assert.Null(item.Sprite);
  }

  private static CreateOrReplaceItemPayload CreateSuperPotionPayload() => new()
  {
    Category = ItemCategory.Medicine,
    Key = "super-potion",
    Name = " Super Potion ",
    Summary = "  Restores more HP.  ",
    Content = "   A spray-type medicine that restores 60 HP to a single Pokémon.   ",
    Price = 700,
    Weight = 30
  };

  private static CreateOrReplaceItemPayload CreateUpdatedPotionPayload() => new()
  {
    Category = ItemCategory.Medicine,
    Key = "potion",
    Name = " Potion ",
    Summary = "  Restores HP better.  ",
    Content = "   A spray-type medicine that restores 30 HP to a single Pokémon.   ",
    Price = 300,
    Weight = 25
  };

  private static void AssertSuperPotion(CreateOrReplaceItemPayload payload, ItemDto item)
  {
    Assert.Equal(payload.Category, item.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), item.Key);
    Assert.Equal(payload.Name?.Trim(), item.Name);
    Assert.Equal(payload.Summary?.Trim(), item.Summary);
    Assert.Equal(payload.Content?.Trim(), item.Content);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Weight, item.Weight);
  }

  private static void AssertUpdatedPotion(CreateOrReplaceItemPayload payload, ItemDto item)
  {
    Assert.Equal(payload.Category, item.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), item.Key);
    Assert.Equal(payload.Name?.Trim(), item.Name);
    Assert.Equal(payload.Summary?.Trim(), item.Summary);
    Assert.Equal(payload.Content?.Trim(), item.Content);
    Assert.Equal(payload.Price, item.Price);
    Assert.Equal(payload.Weight, item.Weight);
  }

  private async Task<AssetDto> UploadSpriteAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.jpg");
    Assert.True(File.Exists(path), $"Add a JPEG file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    return asset;
  }

  private async Task<AssetDto> UploadVideoAsync()
  {
    string path = Path.Combine(AppContext.BaseDirectory, "Assets", "sample.mp4");
    Assert.True(File.Exists(path), $"Add an MP4 file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);
    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);
    Assert.Equal(AssetKind.Video, asset.Kind);
    return asset;
  }
}
