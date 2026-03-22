using Krakenar.Contracts;
using Moq;
using PokeGame.Core.Abilities.Models;

namespace PokeGame.Core.Abilities.Queries;

[Trait(Traits.Category, Categories.Unit)]
public class ReadAbilityQueryHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IAbilityQuerier> _abilityQuerier = new();

  private readonly ReadAbilityQueryHandler _handler;

  public ReadAbilityQueryHandlerTests()
  {
    _handler = new(_abilityQuerier.Object);
  }

  [Fact(DisplayName = "It should return null when no ability was found.")]
  public async Task Given_NoneFound_When_ExecuteAsync_Then_NullReturned()
  {
    ReadAbilityQuery query = new(Guid.Empty, "static");
    Assert.Null(await _handler.HandleAsync(query, _cancellationToken));
  }

  [Fact(DisplayName = "It should return the ability when it was found many times.")]
  public async Task Given_SameFound_When_ExecuteAsync_Then_AbilityReturned()
  {
    AbilityModel ability = new()
    {
      Id = Guid.NewGuid(),
      Key = "static"
    };
    _abilityQuerier.Setup(x => x.ReadAsync(ability.Id, _cancellationToken)).ReturnsAsync(ability);
    _abilityQuerier.Setup(x => x.ReadAsync(ability.Key, _cancellationToken)).ReturnsAsync(ability);

    ReadAbilityQuery query = new(ability.Id, ability.Key);
    AbilityModel? result = await _handler.HandleAsync(query, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(ability, result);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many abilities were found.")]
  public async Task Given_ManyFound_When_ExecuteAsync_Then_TooManyResultsException()
  {
    AbilityModel ability1 = new()
    {
      Id = Guid.NewGuid(),
      Key = "static"
    };
    _abilityQuerier.Setup(x => x.ReadAsync(ability1.Id, _cancellationToken)).ReturnsAsync(ability1);

    AbilityModel ability2 = new()
    {
      Id = Guid.NewGuid(),
      Key = "lightning-rod"
    };
    _abilityQuerier.Setup(x => x.ReadAsync(ability2.Key, _cancellationToken)).ReturnsAsync(ability2);

    ReadAbilityQuery query = new(ability1.Id, ability2.Key);
    var exception = await Assert.ThrowsAsync<TooManyResultsException<AbilityModel>>(async () => await _handler.HandleAsync(query, _cancellationToken));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }
}
