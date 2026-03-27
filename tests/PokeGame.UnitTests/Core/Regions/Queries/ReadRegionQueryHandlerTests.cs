using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadRegionQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IRegionQuerier> _regionQuerier = new();

  private readonly ReadRegionQueryHandler _handler;

  public ReadRegionQueryHandlerTests()
  {
    _handler = new(_regionQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no region was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadRegionQuery query = new(Guid.Empty, "kanto");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the region when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_RegionReturned()
  {
    RegionModel region = new()
    {
      Id = Guid.NewGuid(),
      Key = "kanto"
    };
    _regionQuerier.Setup(x => x.ReadAsync(region.Id, _cancellationToken)).ReturnsAsync(region);
    _regionQuerier.Setup(x => x.ReadAsync(region.Key, _cancellationToken)).ReturnsAsync(region);

    ReadRegionQuery query = new(region.Id, region.Key);
    RegionModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(region, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many regions were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    RegionModel region1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "kanto"
    };
    _regionQuerier.Setup(x => x.ReadAsync(region1.Id, _cancellationToken)).ReturnsAsync(region1);

    RegionModel region2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "johto"
    };
    _regionQuerier.Setup(x => x.ReadAsync(region2.Key, _cancellationToken)).ReturnsAsync(region2);

    ReadRegionQuery query = new(region1.Id, region2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<RegionModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
