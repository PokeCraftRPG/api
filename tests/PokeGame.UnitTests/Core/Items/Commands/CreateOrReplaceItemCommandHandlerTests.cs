using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Items.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceItemCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IItemQuerier> _itemQuerier = new();
  private readonly Mock<IItemRepository> _itemRepository = new();
  private readonly Mock<IMoveManager> _moveManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceItemCommandHandler _handler;

  private readonly Move _thunderPunch;

  public CreateOrReplaceItemCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _itemQuerier.Object, _itemRepository.Object, _moveManager.Object, _permissionService.Object, _storageService.Object);

    _thunderPunch = MoveBuilder.ThunderPunch(_faker, _context.World);
    _moveManager.Setup(x => x.FindAsync(_thunderPunch.Key.Value, "TechnicalMachine.Move", _cancellationToken)).ReturnsAsync(_thunderPunch);
  }

  [Theory(DisplayName = "It should create a new item.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceItemPayload payload = new()
    {
      Key = "tm-999",
      Name = "TM 999 - Thunder Punch",
      Description = "The target is attacked with an electrified punch. This may also leave the target with paralysis.",
      Sprite = "https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/TM",
      Notes = "An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.",
      TechnicalMachine = new TechnicalMachinePropertiesPayload(_thunderPunch.Key.Value)
    };
    CreateOrReplaceItemCommand command = new(payload, id);

    ItemModel model = new();
    _itemQuerier.Setup(x => x.ReadAsync(It.IsAny<Item>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceItemResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Item);

    if (id.HasValue)
    {
      ItemId itemId = new(_context.WorldId, id.Value);
      _itemRepository.Verify(x => x.LoadAsync(itemId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateItem, _cancellationToken), Times.Once());
    _itemQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Item>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Item>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing item.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Item item = new ItemBuilder(_faker).WithWorld(_context.World).IsTechnicalMachine(new TechnicalMachineProperties(_thunderPunch)).ClearChanges().Build();
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    CreateOrReplaceItemPayload payload = new()
    {
      Key = "tm-999",
      Name = "TM 999 - Thunder Punch",
      Description = "The target is attacked with an electrified punch. This may also leave the target with paralysis.",
      Sprite = "https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/TM",
      Notes = "An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.",
      TechnicalMachine = new TechnicalMachinePropertiesPayload(_thunderPunch.Key.Value)
    };
    CreateOrReplaceItemCommand command = new(payload, item.EntityId);

    ItemModel model = new();
    _itemQuerier.Setup(x => x.ReadAsync(item, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceItemResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Item);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, item, _cancellationToken), Times.Once());
    _itemQuerier.Verify(x => x.EnsureUnicityAsync(item, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(item, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the category has changed.")]
  public async Task Given_CategoryChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Item item = new ItemBuilder(_faker).WithWorld(_context.World).IsOtherItem().ClearChanges().Build();
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    CreateOrReplaceItemPayload payload = new()
    {
      Key = "tm-999",
      Name = "TM 999 - Thunder Punch",
      Description = "The target is attacked with an electrified punch. This may also leave the target with paralysis.",
      Sprite = "https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/TM",
      Notes = "An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting.",
      TechnicalMachine = new TechnicalMachinePropertiesPayload(_thunderPunch.Key.Value)
    };
    CreateOrReplaceItemCommand command = new(payload, item.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<ItemCategory>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(item.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Item", exception.EntityKind);
    Assert.Equal(item.EntityId, exception.EntityId);
    Assert.Equal(item.Category, exception.ExpectedValue);
    Assert.Equal(ItemCategory.TechnicalMachine, exception.AttemptedValue);
    Assert.Equal("TechnicalMachine", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceItemPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Price = 0,
      Sprite = "invalid",
      Url = "invalid",
      BattleItem = new BattleItemPropertiesModel(attack: -10, defense: 10),
      Berry = new BerryPropertiesModel(healing: 1000, isHealingPercentage: true),
      Medicine = new MedicinePropertiesModel(statusCondition: StatusCondition.Poison, allConditions: true),
      PokeBall = new PokeBallPropertiesModel(catchMultiplier: -1, friendshipMultiplier: 0),
      TechnicalMachine = new TechnicalMachinePropertiesPayload()
    };
    CreateOrReplaceItemCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(13, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Price.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "BattleItem.Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "BattleItem.Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Berry.Healing");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NullValidator" && e.PropertyName == "Medicine.StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "PokeBall.CatchMultiplier");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "PokeBall.FriendshipMultiplier");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "TechnicalMachine.Move");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "CreateOrReplaceItemValidator");
  }
}
