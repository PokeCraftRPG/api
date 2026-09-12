using PokeGame.Core;
using PokeGame.Core.Pokemon.Models;

namespace PokeGame.Pokemon;

public class PokemonPayloadTests : UnitTests
{
  [Fact(DisplayName = "It should throw InvalidCommandException when egg cycles and experience are both set.")]
  public void Given_EggAndExperience_When_ValidateCreate_Then_InvalidCommandException()
  {
    CreatePokemonPayload payload = new()
    {
      EggCycles = 5,
      Experience = 100
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the receive location is empty.")]
  public void Given_EmptyLocation_When_ValidateReceive_Then_InvalidCommandException()
  {
    ReceivePokemonPayload payload = new()
    {
      Location = string.Empty
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the catch location is empty.")]
  public void Given_EmptyLocation_When_ValidateCatch_Then_InvalidCommandException()
  {
    CatchPokemonPayload payload = new()
    {
      Location = string.Empty
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Theory(DisplayName = "It should throw InvalidCommandException when the trade Pokémon IDs are invalid.")]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(2)]
  public void Given_InvalidPokemonIds_When_ValidateTrade_Then_InvalidCommandException(int count)
  {
    Guid id = Guid.NewGuid();
    List<Guid> pokemonIds = count switch
    {
      0 => [],
      1 => [id],
      _ => [id, id]
    };

    TradePokemonPayload payload = new()
    {
      PokemonIds = pokemonIds,
      Location = "Pokémon Center"
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should accept a valid trade payload.")]
  public void Given_TwoDistinctIds_When_ValidateTrade_Then_Valid()
  {
    new TradePokemonPayload
    {
      PokemonIds = [Guid.NewGuid(), Guid.NewGuid()],
      Location = "Pokémon Center"
    }.Validate();
  }
}
