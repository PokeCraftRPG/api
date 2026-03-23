using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Moves.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateMoveCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMoveRepository> _moveRepository = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IMoveQuerier> _moveQuerier = new();

  private readonly TestContext _context;
  private readonly UpdateMoveCommandHandler _handler;

  public UpdateMoveCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _moveQuerier.Object, _moveRepository.Object, _permissionService.Object, _storageService.Object);
  }

  [Fact(DisplayName = "It should return null when the move does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateMovePayload payload = new();
    UpdateMoveCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateMovePayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Accuracy = new Optional<byte?>(byte.MaxValue),
      Power = new Optional<byte?>(0),
      PowerPoints = 0,
      Url = new Optional<string>("invalid")
    };
    UpdateMoveCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Accuracy.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Power.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "PowerPoints.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing move.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Move move = new MoveBuilder(_faker).WithWorld(_context.World).WithType(PokemonType.Electric).WithCategory(MoveCategory.Special).ClearChanges().Build();
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    UpdateMovePayload payload = new()
    {
      Key = "thunder-shock",
      Name = new Optional<string>("Thunder Shock"),
      Description = new Optional<string>("The user attacks the target with a jolt of electricity. This may also leave the target with paralysis."),
      Accuracy = new Optional<byte?>(100),
      Power = new Optional<byte?>(40),
      PowerPoints = 30,
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)"),
      Notes = new Optional<string>("Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed.")
    };
    UpdateMoveCommand command = new(move.EntityId, payload);

    MoveModel model = new();
    _moveQuerier.Setup(x => x.ReadAsync(move, _cancellationToken)).ReturnsAsync(model);

    MoveModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, move, _cancellationToken), Times.Once());
    _moveQuerier.Verify(x => x.EnsureUnicityAsync(move, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(move, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
