using Bogus;
using Krakenar.Contracts.Users;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateTrainerCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ITrainerQuerier> _trainerQuerier = new();
  private readonly Mock<ITrainerRepository> _trainerRepository = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IUserGateway> _userGateway = new();

  private readonly TestContext _context;
  private readonly UpdateTrainerCommandHandler _handler;

  public UpdateTrainerCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _storageService.Object, _trainerQuerier.Object, _trainerRepository.Object, _userGateway.Object);
  }

  [Fact(DisplayName = "It should return null when the trainer does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateTrainerPayload payload = new();
    UpdateTrainerCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw UserNotFoundException when the user was not found.")]
  public async Task Given_UserNotFound_When_HandleAsync_Then_UserNotFoundException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    UpdateTrainerPayload payload = new()
    {
      OwnerId = new Optional<Guid?>(Guid.NewGuid())
    };
    UpdateTrainerCommand command = new(trainer.EntityId, payload);

    var exception = await Assert.ThrowsAsync<UserNotFoundException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(payload.OwnerId.Value, exception.UserId);
    Assert.Equal("OwnerId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateTrainerPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Gender = (TrainerGender)(-2),
      Money = -1000,
      Sprite = new Optional<string>("invalid"),
      Url = new Optional<string>("invalid")
    };
    UpdateTrainerCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Gender.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanOrEqualValidator" && e.PropertyName == "Money.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing trainer.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    User owner = new UserBuilder(_faker).Build();
    trainer.SetOwnership(owner.GetUserId(), _context.UserId);

    UpdateTrainerPayload payload = new()
    {
      OwnerId = new Optional<Guid?>(null),
      Key = "ash-ketchum",
      Name = new Optional<string>("Ash Ketchum"),
      Description = new Optional<string>("Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations."),
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = new Optional<string>("https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png"),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum"),
      Notes = new Optional<string>("Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration.")
    };
    UpdateTrainerCommand command = new(trainer.EntityId, payload);

    TrainerModel model = new();
    _trainerQuerier.Setup(x => x.ReadAsync(trainer, _cancellationToken)).ReturnsAsync(model);

    TrainerModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, trainer, _cancellationToken), Times.Once());
    _trainerQuerier.Verify(x => x.EnsureUnicityAsync(trainer, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(trainer, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());

    Assert.Null(trainer.OwnerId);
    Assert.Contains(trainer.Changes, change => change is TrainerOwnershipChanged ownership && ownership.OwnerId is null && ownership.ActorId == _context.UserId.ActorId);
  }
}
