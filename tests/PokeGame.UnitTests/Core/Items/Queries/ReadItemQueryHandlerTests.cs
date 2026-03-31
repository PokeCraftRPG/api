using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadItemQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IItemQuerier> _itemQuerier = new();

  private readonly ReadItemQueryHandler _handler;

  public ReadItemQueryHandlerTests()
  {
    _handler = new(_itemQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no item was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadItemQuery query = new(Guid.Empty, "potion");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the item when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_ItemReturned()
  {
    ItemModel item = new()
    {
      Id = Guid.NewGuid(),
      Key = "potion"
    };
    _itemQuerier.Setup(x => x.ReadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);
    _itemQuerier.Setup(x => x.ReadAsync(item.Key, _cancellationToken)).ReturnsAsync(item);

    ReadItemQuery query = new(item.Id, item.Key);
    ItemModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(item, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many items were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    ItemModel item1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "potion"
    };
    _itemQuerier.Setup(x => x.ReadAsync(item1.Id, _cancellationToken)).ReturnsAsync(item1);

    ItemModel item2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "revive"
    };
    _itemQuerier.Setup(x => x.ReadAsync(item2.Key, _cancellationToken)).ReturnsAsync(item2);

    ReadItemQuery query = new(item1.Id, item2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<ItemModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
