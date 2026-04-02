using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateEvolutionCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IEvolutionQuerier> _evolutionQuerier = new();
  private readonly Mock<IEvolutionRepository> _evolutionRepository = new();
  private readonly Mock<IItemManager> _itemManager = new();
  private readonly Mock<IMoveManager> _moveManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateEvolutionCommandHandler _handler;

  private readonly World _world;

  public UpdateEvolutionCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
      _evolutionQuerier.Object,
      _evolutionRepository.Object,
      _itemManager.Object,
      _moveManager.Object,
      _permissionService.Object,
      _storageService.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
  }

  [Fact(DisplayName = "It should return null when the evolution does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateEvolutionPayload payload = new();
    UpdateEvolutionCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateEvolutionPayload payload = new()
    {
      Level = new Optional<int?>(0),
      Gender = new Optional<PokemonGender?>((PokemonGender)(-1)),
      Location = new Optional<string>(_faker.Random.String(999, 'a', 'z')),
      TimeOfDay = new Optional<TimeOfDay?>((TimeOfDay)(-1))
    };
    UpdateEvolutionCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(4, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Level.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Gender.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Location.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "TimeOfDay.Value");
  }

  [Fact(DisplayName = "It should update the existing evolution.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world);
    _evolutionRepository.Setup(x => x.LoadAsync(evolution.Id, _cancellationToken)).ReturnsAsync(evolution);

    UpdateEvolutionPayload payload = new()
    {
      HeldItem = new Optional<string>("oran-berry"),
      KnownMove = new Optional<string>("thunder-punch")
    };
    UpdateEvolutionCommand command = new(evolution.EntityId, payload);

    Item oranBerry = ItemBuilder.OranBerry(_faker, _world);
    _itemManager.Setup(x => x.FindAsync(oranBerry.Key.Value, nameof(payload.HeldItem), _cancellationToken)).ReturnsAsync(oranBerry);

    Move thunderPunch = MoveBuilder.ThunderPunch(_faker, _world);
    _moveManager.Setup(x => x.FindAsync(thunderPunch.Key.Value, nameof(payload.KnownMove), _cancellationToken)).ReturnsAsync(thunderPunch);

    EvolutionModel model = new();
    _evolutionQuerier.Setup(x => x.ReadAsync(evolution, _cancellationToken)).ReturnsAsync(model);

    EvolutionModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, evolution, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(evolution, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());

    Assert.Equal(oranBerry.Id, evolution.HeldItemId);
    Assert.Equal(thunderPunch.Id, evolution.KnownMoveId);
  }
}
