using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceTrainerCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ITrainerQuerier> _trainerQuerier = new();
  private readonly Mock<ITrainerRepository> _trainerRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceTrainerCommandHandler _handler;

  public CreateOrReplaceTrainerCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _storageService.Object, _trainerQuerier.Object, _trainerRepository.Object);
  }

  [Theory(DisplayName = "It should create a new trainer.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceTrainerPayload payload = new()
    {
      License = "Q-123456-3",
      Key = "ash-ketchum",
      Name = "Ash Ketchum",
      Description = "Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.",
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = "https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum",
      Notes = "Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration."
    };
    CreateOrReplaceTrainerCommand command = new(payload, id);

    TrainerModel model = new();
    _trainerQuerier.Setup(x => x.ReadAsync(It.IsAny<Trainer>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceTrainerResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Trainer);

    if (id.HasValue)
    {
      TrainerId trainerId = new(_context.WorldId, id.Value);
      _trainerRepository.Verify(x => x.LoadAsync(trainerId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateTrainer, _cancellationToken), Times.Once());
    _trainerQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Trainer>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Trainer>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing trainer.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    CreateOrReplaceTrainerPayload payload = new()
    {
      License = trainer.License.Value,
      Key = "ash-ketchum",
      Name = "Ash Ketchum",
      Description = "Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.",
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = "https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum",
      Notes = "Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration."
    };
    CreateOrReplaceTrainerCommand command = new(payload, trainer.EntityId);

    TrainerModel model = new();
    _trainerQuerier.Setup(x => x.ReadAsync(trainer, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceTrainerResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Trainer);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, trainer, _cancellationToken), Times.Once());
    _trainerQuerier.Verify(x => x.EnsureUnicityAsync(trainer, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(trainer, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the license has changed.")]
  public async Task Given_LicenseChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    CreateOrReplaceTrainerPayload payload = new()
    {
      License = "Q-940401-9",
      Key = "ash-ketchum",
      Name = "Ash Ketchum",
      Description = "Ash is a legendary Trainer known for Pikachu, constant youth, and mastering multiple battle styles across regions and generations.",
      Gender = TrainerGender.Male,
      Money = 987654,
      Sprite = "https://archives.bulbagarden.net/media/upload/thumb/c/cd/Ash_JN.png/800px-Ash_JN.png",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Ash_Ketchum",
      Notes = "Ash provides rich lore: timeless setting, unique feats, and rare mechanics (Mega, Z-Moves, Dynamax) useful for narrative and rule inspiration."
    };
    CreateOrReplaceTrainerCommand command = new(payload, trainer.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<string>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(trainer.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Trainer", exception.EntityKind);
    Assert.Equal(trainer.EntityId, exception.EntityId);
    Assert.Equal(trainer.License.Value, exception.ExpectedValue);
    Assert.Equal(payload.License, exception.AttemptedValue);
    Assert.Equal("License", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceTrainerPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Gender = (TrainerGender)2,
      Money = -1000,
      Sprite = "invalid",
      Url = "invalid"
    };
    CreateOrReplaceTrainerCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(7, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "License");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Gender");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanOrEqualValidator" && e.PropertyName == "Money");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
