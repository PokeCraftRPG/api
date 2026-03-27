using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadVarietyQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IVarietyQuerier> _varietyQuerier = new();

  private readonly ReadVarietyQueryHandler _handler;

  public ReadVarietyQueryHandlerTests()
  {
    _handler = new(_varietyQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no variety was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadVarietyQuery query = new(Guid.Empty, "pikachu");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the variety when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_VarietyReturned()
  {
    VarietyModel variety = new()
    {
      Id = Guid.NewGuid(),
      Key = "pikachu"
    };
    _varietyQuerier.Setup(x => x.ReadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);
    _varietyQuerier.Setup(x => x.ReadAsync(variety.Key, _cancellationToken)).ReturnsAsync(variety);

    ReadVarietyQuery query = new(variety.Id, variety.Key);
    VarietyModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(variety, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many varieties were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    VarietyModel variety1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "pikachu"
    };
    _varietyQuerier.Setup(x => x.ReadAsync(variety1.Id, _cancellationToken)).ReturnsAsync(variety1);

    VarietyModel variety2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "eevee"
    };
    _varietyQuerier.Setup(x => x.ReadAsync(variety2.Key, _cancellationToken)).ReturnsAsync(variety2);

    ReadVarietyQuery query = new(variety1.Id, variety2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<VarietyModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
