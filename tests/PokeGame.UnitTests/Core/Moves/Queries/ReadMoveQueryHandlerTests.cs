using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Moves.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadMoveQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IMoveQuerier> _moveQuerier = new();

  private readonly ReadMoveQueryHandler _handler;

  public ReadMoveQueryHandlerTests()
  {
    _handler = new(_moveQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no move was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadMoveQuery query = new(Guid.Empty, "kanto");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the move when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_MoveReturned()
  {
    MoveModel move = new()
    {
      Id = Guid.NewGuid(),
      Key = "kanto"
    };
    _moveQuerier.Setup(x => x.ReadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);
    _moveQuerier.Setup(x => x.ReadAsync(move.Key, _cancellationToken)).ReturnsAsync(move);

    ReadMoveQuery query = new(move.Id, move.Key);
    MoveModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(move, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many abilities were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    MoveModel move1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "kanto"
    };
    _moveQuerier.Setup(x => x.ReadAsync(move1.Id, _cancellationToken)).ReturnsAsync(move1);

    MoveModel move2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "johto"
    };
    _moveQuerier.Setup(x => x.ReadAsync(move2.Key, _cancellationToken)).ReturnsAsync(move2);

    ReadMoveQuery query = new(move1.Id, move2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<MoveModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
