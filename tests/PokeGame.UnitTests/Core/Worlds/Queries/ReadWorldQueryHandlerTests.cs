using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadWorldQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IWorldQuerier> _worldQuerier = new();

  private readonly ReadWorldQueryHandler _handler;

  public ReadWorldQueryHandlerTests()
  {
    _handler = new(_worldQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no world was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadWorldQuery query = new(Guid.Empty, "the-new-world");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the world when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_WorldReturned()
  {
    WorldModel world = new()
    {
      Id = Guid.NewGuid(),
      Key = "the-new-world"
    };
    _worldQuerier.Setup(x => x.ReadAsync(world.Id, _cancellationToken)).ReturnsAsync(world);
    _worldQuerier.Setup(x => x.ReadAsync(world.Key, _cancellationToken)).ReturnsAsync(world);

    ReadWorldQuery query = new(world.Id, world.Key);
    WorldModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(world, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many worlds were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    WorldModel world1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "the-old-world"
    };
    _worldQuerier.Setup(x => x.ReadAsync(world1.Id, _cancellationToken)).ReturnsAsync(world1);

    WorldModel world2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "the-new-world"
    };
    _worldQuerier.Setup(x => x.ReadAsync(world2.Key, _cancellationToken)).ReturnsAsync(world2);

    ReadWorldQuery query = new(world1.Id, world2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<WorldModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
