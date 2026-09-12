using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;

namespace PokeGame.Pokemon;

public class SpecimenTradeTests : UnitTests
{
  [Fact(DisplayName = "It should trade two Pokémon and keep each Poké Ball and original trainer.")]
  public void Given_DifferentOwners_When_Trade_Then_OwnersSwapped()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source", pokeBall: Catalog.MasterBall);
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "target", pokeBall: Catalog.PokeBall);

    source.Trade(target, Catalog.PokemonCenter);

    Assert.Equal(OwnershipEvent.Traded, source.Ownership!.Event);
    Assert.Equal(Catalog.Blue.Id, source.Ownership.TrainerId);
    Assert.Equal(Catalog.MasterBall.Id, source.Ownership.PokeBallId);
    Assert.Equal(Catalog.Red.Id, source.OriginalTrainerId);
    Assert.Equal(Catalog.PokemonCenter, source.Ownership.MetAt);

    Assert.Equal(OwnershipEvent.Traded, target.Ownership!.Event);
    Assert.Equal(Catalog.Red.Id, target.Ownership.TrainerId);
    Assert.Equal(Catalog.PokeBall.Id, target.Ownership.PokeBallId);
    Assert.Equal(Catalog.Blue.Id, target.OriginalTrainerId);

    PokemonTraded sourceEvent = source.LastChange<PokemonTraded>();
    Assert.Equal(Catalog.Blue.Id, sourceEvent.TrainerId);
    PokemonTraded targetEvent = target.LastChange<PokemonTraded>();
    Assert.Equal(Catalog.Red.Id, targetEvent.TrainerId);
  }

  [Fact(DisplayName = "It should trade eggs without setting the original trainer.")]
  public void Given_Eggs_When_Trade_Then_OriginalTrainerRemainsUnset()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source-egg", eggCycles: 5);
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "target-egg", eggCycles: 5);

    source.Trade(target, Catalog.PokemonCenter);

    Assert.Null(source.OriginalTrainerId);
    Assert.Null(target.OriginalTrainerId);
    Assert.Equal(Catalog.Blue.Id, source.Ownership!.TrainerId);
    Assert.Equal(Catalog.Red.Id, target.Ownership!.TrainerId);
  }

  [Fact(DisplayName = "It should throw PokemonCannotBeTradedWithItselfException when trading a Pokémon with itself.")]
  public void Given_SamePokemon_When_Trade_Then_PokemonCannotBeTradedWithItselfException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    Assert.Throws<PokemonCannotBeTradedWithItselfException>(() => pokemon.Trade(pokemon, Catalog.PokemonCenter));
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when a Pokémon is wild.")]
  public void Given_WildPokemon_When_Trade_Then_PokemonHasNoOwnerException()
  {
    Specimen source = Catalog.CreatePokemon("wild");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "owned");

    PokemonHasNoOwnerException exception = Assert.Throws<PokemonHasNoOwnerException>(
      () => source.Trade(target, Catalog.PokemonCenter));
    Assert.Equal(source.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonTradeRequiresDifferentOwnersException when both Pokémon have the same owner.")]
  public void Given_SameOwner_When_Trade_Then_PokemonTradeRequiresDifferentOwnersException()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Red, "target");

    PokemonTradeRequiresDifferentOwnersException exception = Assert.Throws<PokemonTradeRequiresDifferentOwnersException>(
      () => source.Trade(target, Catalog.PokemonCenter));
    Assert.Equal(source.EntityId, exception.Data["SourcePokemonId"]);
    Assert.Equal(target.EntityId, exception.Data["TargetPokemonId"]);
  }
}
