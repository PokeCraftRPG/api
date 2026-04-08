using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Core.Pokemon.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadPokemonQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();

  private readonly ReadPokemonQueryHandler _handler;

  public ReadPokemonQueryHandlerTests()
  {
    _handler = new(_pokemonQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no pokemon was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadPokemonQuery query = new(Guid.Empty, "briquet");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the pokemon when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_PokemonReturned()
  {
    PokemonModel pokemon = new()
    {
      Id = Guid.NewGuid(),
      Key = "briquet"
    };
    _pokemonQuerier.Setup(x => x.ReadAsync(pokemon.Id, _cancellationToken)).ReturnsAsync(pokemon);
    _pokemonQuerier.Setup(x => x.ReadAsync(pokemon.Key, _cancellationToken)).ReturnsAsync(pokemon);

    ReadPokemonQuery query = new(pokemon.Id, pokemon.Key);
    PokemonModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(pokemon, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many pokemons were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    PokemonModel pokemon1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "briquet"
    };
    _pokemonQuerier.Setup(x => x.ReadAsync(pokemon1.Id, _cancellationToken)).ReturnsAsync(pokemon1);

    PokemonModel pokemon2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "hedwidge"
    };
    _pokemonQuerier.Setup(x => x.ReadAsync(pokemon2.Key, _cancellationToken)).ReturnsAsync(pokemon2);

    ReadPokemonQuery query = new(pokemon1.Id, pokemon2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<PokemonModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
