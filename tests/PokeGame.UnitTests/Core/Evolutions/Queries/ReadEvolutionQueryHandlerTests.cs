using Moq;

namespace PokeGame.Core.Evolutions.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadEvolutionQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IEvolutionQuerier> _evolutionQuerier = new();

  private readonly ReadEvolutionQueryHandler _handler;

  public ReadEvolutionQueryHandlerTests()
  {
    _handler = new(_evolutionQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no evolution was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadEvolutionQuery query = new(Guid.Empty);
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }
}
