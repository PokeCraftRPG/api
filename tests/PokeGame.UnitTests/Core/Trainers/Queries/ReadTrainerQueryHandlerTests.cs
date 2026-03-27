using Bogus;
using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Trainers.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadTrainerQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<ITrainerQuerier> _trainerQuerier = new();

  private readonly ReadTrainerQueryHandler _handler;

  public ReadTrainerQueryHandlerTests()
  {
    _handler = new(_trainerQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no trainer was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadTrainerQuery query = new(Guid.Empty, "Q-123456-3", "ash-ketchum");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the trainer when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_TrainerReturned()
  {
    TrainerModel trainer = new()
    {
      Id = Guid.NewGuid(),
      License = _faker.TrainerLicense().Value,
      Key = "ash-ketchum"
    };
    _trainerQuerier.Setup(x => x.ReadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(trainer);
    _trainerQuerier.Setup(x => x.ReadAsync(trainer.Key, _cancellationToken)).ReturnsAsync(trainer);

    ReadTrainerQuery query = new(trainer.Id, trainer.License, trainer.Key);
    TrainerModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(trainer, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many trainers were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    TrainerModel trainer1 = new()
    {
      Id = Guid.NewGuid(),
      License = _faker.TrainerLicense().Value,
      Key = "ash-ketchum"
    };
    _trainerQuerier.Setup(x => x.ReadAsync(trainer1.Id, _cancellationToken)).ReturnsAsync(trainer1);

    TrainerModel trainer2 = new()
    {
      Id = Guid.NewGuid(),
      License = _faker.TrainerLicense().Value,
      Key = "brock"
    };
    _trainerQuerier.Setup(x => x.ReadByLicenseAsync(trainer2.License, _cancellationToken)).ReturnsAsync(trainer2);

    TrainerModel trainer3 = new()
    {
      Id = Guid.NewGuid(),
      License = _faker.TrainerLicense().Value,
      Key = "misty"
    };
    _trainerQuerier.Setup(x => x.ReadAsync(trainer3.Key, _cancellationToken)).ReturnsAsync(trainer3);

    ReadTrainerQuery query = new(trainer1.Id, trainer2.License, trainer3.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<TrainerModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(3, exception.ActualCount);
  }
}
