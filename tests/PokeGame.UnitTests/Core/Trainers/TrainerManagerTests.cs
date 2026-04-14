using Bogus;
using Moq;
using PokeGame.Builders;

namespace PokeGame.Core.Trainers;

[Trait(Traits.Category, Categories.Unit)]
public class TrainerManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<ITrainerQuerier> _trainerQuerier = new();
  private readonly Mock<ITrainerRepository> _trainerRepository = new();

  private readonly TestContext _context;
  private readonly TrainerManager _manager;

  public TrainerManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _trainerQuerier.Object, _trainerRepository.Object);
  }

  [Fact(DisplayName = "FindAsync: it should return the trainer found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_TrainerReturned()
  {
    Trainer trainer = TrainerBuilder.AshKetchum(_faker, _context.World);
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    Trainer found = await _manager.FindAsync($"  {trainer.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(trainer, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the trainer found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_TrainerReturned()
  {
    Trainer trainer = TrainerBuilder.AshKetchum(_faker, _context.World);
    _trainerRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);

    string key = $"  {trainer.Key.Value.ToUpperInvariant()}  ";
    _trainerQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(trainer.Id);

    Trainer found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(trainer, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the trainer was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    Trainer trainer = TrainerBuilder.AshKetchum(_faker, _context.World);
    _trainerQuerier.Setup(x => x.FindIdAsync(trainer.Key.Value, _cancellationToken)).ReturnsAsync(trainer.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(trainer.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The trainer 'Id={trainer.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw TrainerNotFoundException when the trainer was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_TrainerNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<TrainerNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Trainer);
    Assert.Equal(PropertyName, exception.PropertyName);

    _trainerRepository.Verify(x => x.LoadAsync(new TrainerId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _trainerQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }
}
