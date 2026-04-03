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
public class UpdateItemCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IItemQuerier> _itemQuerier = new();
  private readonly Mock<IItemRepository> _itemRepository = new();
  private readonly Mock<IMoveManager> _moveManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateItemCommandHandler _handler;

  private readonly Move _thunderPunch;

  public UpdateItemCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _itemQuerier.Object, _itemRepository.Object, _moveManager.Object, _permissionService.Object, _storageService.Object);

    _thunderPunch = MoveBuilder.ThunderPunch(_faker, _context.World);
    _moveManager.Setup(x => x.FindAsync(_thunderPunch.Key.Value, "TechnicalMachine.Move", _cancellationToken)).ReturnsAsync(_thunderPunch);
  }

  [Fact(DisplayName = "It should return null when the item does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateItemPayload payload = new();
    UpdateItemCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateItemPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Price = new Optional<int?>(0),
      Sprite = new Optional<string>("invalid"),
      Url = new Optional<string>("invalid"),
      BattleItem = new BattleItemPropertiesModel(attack: -10, defense: 10),
      Berry = new BerryPropertiesModel(healing: 1000, isHealingPercentage: true),
      Medicine = new MedicinePropertiesModel(statusCondition: StatusCondition.Poison, allConditions: true),
      PokeBall = new PokeBallPropertiesModel(catchMultiplier: -1, friendshipMultiplier: 0),
      TechnicalMachine = new TechnicalMachinePropertiesPayload()
    };
    UpdateItemCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(13, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "Price.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "BattleItem.Attack");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "BattleItem.Defense");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Berry.Healing");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NullValidator" && e.PropertyName == "Medicine.StatusCondition");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "PokeBall.CatchMultiplier");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "PokeBall.FriendshipMultiplier");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "TechnicalMachine.Move");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UpdateItemValidator");
  }

  [Fact(DisplayName = "It should update the existing item.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Item item = new ItemBuilder(_faker).WithWorld(_context.World).IsTechnicalMachine(new TechnicalMachineProperties(_thunderPunch)).ClearChanges().Build();
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    UpdateItemPayload payload = new()
    {
      Key = "tm-999",
      Name = new Optional<string>("TM 999 - Thunder Punch"),
      Description = new Optional<string>("The target is attacked with an electrified punch. This may also leave the target with paralysis."),
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/f/f4/Bag_TM_Electric_ZA_Sprite.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/TM"),
      Notes = new Optional<string>("An item used to teach Pokémon moves. Usually single-use (varies by generation), sometimes reusable, and widely obtainable via shops, rewards, or crafting."),
      TechnicalMachine = new TechnicalMachinePropertiesPayload(_thunderPunch.Key.Value)
    };
    UpdateItemCommand command = new(item.EntityId, payload);

    ItemModel model = new();
    _itemQuerier.Setup(x => x.ReadAsync(item, _cancellationToken)).ReturnsAsync(model);

    ItemModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, item, _cancellationToken), Times.Once());
    _itemQuerier.Verify(x => x.EnsureUnicityAsync(item, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(item, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
