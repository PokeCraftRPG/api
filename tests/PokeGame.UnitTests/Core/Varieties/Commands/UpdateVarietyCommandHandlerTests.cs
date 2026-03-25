using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateVarietyCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IVarietyManager> _varietyManager = new();
  private readonly Mock<IVarietyQuerier> _varietyQuerier = new();
  private readonly Mock<IVarietyRepository> _varietyRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateVarietyCommandHandler _handler;

  public UpdateVarietyCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _storageService.Object, _varietyManager.Object, _varietyQuerier.Object, _varietyRepository.Object);
  }

  [Fact(DisplayName = "It should return null when the variety does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateVarietyPayload payload = new();
    UpdateVarietyCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateVarietyPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Genus = new Optional<string>(_faker.Random.String(Genus.MaximumLength + 1, 'a', 'z')),
      GenderRatio = new Optional<int?>(10),
      Url = new Optional<string>("invalid")
    };
    payload.Moves.Add(new VarietyMovePayload());
    payload.Moves.Add(new VarietyMovePayload("thunder-shock", level: -1));
    payload.Moves.Add(new VarietyMovePayload("sweet-kiss", level: 101));
    UpdateVarietyCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(8, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Genus.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "GenderRatio.Value.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Moves[0].Move");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Moves[1].Level.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Moves[2].Level.Value");
  }

  [Fact(DisplayName = "It should update the existing variety.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Variety variety = new VarietyBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);

    UpdateVarietyPayload payload = new()
    {
      Key = "pikachu",
      Name = new Optional<string>("Pikachu"),
      Genus = new Optional<string>("Mouse"),
      Description = new Optional<string>("It has small electric sacs on both its cheeks. When in a tough spot, this Pokémon discharges electricity."),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("This is the default variety.")
    };
    payload.Moves.Add(new VarietyMovePayload("thunder-shock", 1));
    payload.Moves.Add(new VarietyMovePayload(Guid.NewGuid().ToString(), 0));
    UpdateVarietyCommand command = new(variety.EntityId, payload);

    Dictionary<MoveId, int?> moves = new()
    {
      [MoveId.NewId(_context.WorldId)] = 1,
      [MoveId.NewId(_context.WorldId)] = 0
    };
    _varietyManager.Setup(x => x.FindMovesAsync(payload.Moves, "Moves", _cancellationToken)).ReturnsAsync(moves);

    VarietyModel model = new();
    _varietyQuerier.Setup(x => x.ReadAsync(variety, _cancellationToken)).ReturnsAsync(model);

    VarietyModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, variety, _cancellationToken), Times.Once());
    _varietyQuerier.Verify(x => x.EnsureUnicityAsync(variety, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(variety, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
