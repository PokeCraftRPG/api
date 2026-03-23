using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Moves.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceMoveCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMoveQuerier> _moveQuerier = new();
  private readonly Mock<IMoveRepository> _moveRepository = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceMoveCommandHandler _handler;

  public CreateOrReplaceMoveCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _moveQuerier.Object, _moveRepository.Object, _permissionService.Object, _storageService.Object);
  }

  [Theory(DisplayName = "It should create a new move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Electric,
      Category = MoveCategory.Special,
      Key = "thunder-shock",
      Name = "Thunder Shock",
      Description = "The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed."
    };
    CreateOrReplaceMoveCommand command = new(payload, id);

    MoveModel model = new();
    _moveQuerier.Setup(x => x.ReadAsync(It.IsAny<Move>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceMoveResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Move);

    if (id.HasValue)
    {
      MoveId moveId = new(_context.WorldId, id.Value);
      _moveRepository.Verify(x => x.LoadAsync(moveId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateMove, _cancellationToken), Times.Once());
    _moveQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Move>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Move>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing move.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Move move = new MoveBuilder(_faker).WithWorld(_context.World).WithType(PokemonType.Electric).WithCategory(MoveCategory.Special).ClearChanges().Build();
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    CreateOrReplaceMovePayload payload = new()
    {
      Type = move.Type,
      Category = move.Category,
      Key = "thunder-shock",
      Name = "Thunder Shock",
      Description = "The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed."
    };
    CreateOrReplaceMoveCommand command = new(payload, move.EntityId);

    MoveModel model = new();
    _moveQuerier.Setup(x => x.ReadAsync(move, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceMoveResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Move);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, move, _cancellationToken), Times.Once());
    _moveQuerier.Verify(x => x.EnsureUnicityAsync(move, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(move, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the category has changed.")]
  public async Task Given_CategoryChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Move move = new MoveBuilder(_faker).WithWorld(_context.World).WithCategory(MoveCategory.Special).ClearChanges().Build();
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Electric,
      Category = MoveCategory.Special,
      Key = "thunder-shock",
      Name = "Thunder Shock",
      Description = "The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed."
    };
    CreateOrReplaceMoveCommand command = new(payload, move.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<PokemonType>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(move.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Move", exception.EntityKind);
    Assert.Equal(move.EntityId, exception.EntityId);
    Assert.Equal(move.Type, exception.ExpectedValue);
    Assert.Equal(payload.Type, exception.AttemptedValue);
    Assert.Equal("Type", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the type has changed.")]
  public async Task Given_TypeChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Move move = new MoveBuilder(_faker).WithWorld(_context.World).WithType(PokemonType.Electric).ClearChanges().Build();
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Electric,
      Category = MoveCategory.Special,
      Key = "thunder-shock",
      Name = "Thunder Shock",
      Description = "The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed."
    };
    CreateOrReplaceMoveCommand command = new(payload, move.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<MoveCategory>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(move.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Move", exception.EntityKind);
    Assert.Equal(move.EntityId, exception.EntityId);
    Assert.Equal(move.Category, exception.ExpectedValue);
    Assert.Equal(payload.Category, exception.AttemptedValue);
    Assert.Equal("Category", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = (PokemonType)(-1),
      Category = (MoveCategory)(-1),
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Accuracy = byte.MaxValue,
      Power = 0,
      Url = "invalid"
    };
    CreateOrReplaceMoveCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(8, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Type");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Category");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Accuracy.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Power.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "PowerPoints");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
