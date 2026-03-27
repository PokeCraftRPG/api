using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadSpeciesQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<ISpeciesQuerier> _speciesQuerier = new();

  private readonly ReadSpeciesQueryHandler _handler;

  public ReadSpeciesQueryHandlerTests()
  {
    _handler = new(_speciesQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no species was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadSpeciesQuery query = new(Guid.Empty, 25, "pikachu");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the species when it was found by id and key.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_SpeciesReturned()
  {
    SpeciesModel species = new()
    {
      Id = Guid.NewGuid(),
      Number = 25,
      Key = "pikachu"
    };
    _speciesQuerier.Setup(x => x.ReadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);
    _speciesQuerier.Setup(x => x.ReadAsync(species.Key, _cancellationToken)).ReturnsAsync(species);

    ReadSpeciesQuery query = new(species.Id, species.Number, species.Key);
    SpeciesModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(species, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many species were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    SpeciesModel species1 = new()
    {
      Id = Guid.NewGuid(),
      Number = 25,
      Key = "pikachu"
    };
    _speciesQuerier.Setup(x => x.ReadAsync(species1.Id, _cancellationToken)).ReturnsAsync(species1);

    SpeciesModel species2 = new()
    {
      Id = Guid.NewGuid(),
      Number = 26,
      Key = "raichu"
    };
    _speciesQuerier.Setup(x => x.ReadAsync(species2.Number, _cancellationToken)).ReturnsAsync(species2);

    SpeciesModel species3 = new()
    {
      Id = Guid.NewGuid(),
      Number = 133,
      Key = "eevee"
    };
    _speciesQuerier.Setup(x => x.ReadAsync(species3.Key, _cancellationToken)).ReturnsAsync(species3);

    ReadSpeciesQuery query = new(species1.Id, species2.Number, species3.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<SpeciesModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(3, exception.ActualCount);
  }
}
