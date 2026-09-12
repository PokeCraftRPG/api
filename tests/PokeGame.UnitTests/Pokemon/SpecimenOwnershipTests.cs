using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;

namespace PokeGame.Pokemon;

public class SpecimenOwnershipTests : UnitTests
{
  [Fact(DisplayName = "It should receive a wild Pokémon and set the original trainer.")]
  public void Given_WildPokemon_When_Receive_Then_OwnedAndOriginalTrainerSet()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    pokemon.Receive(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(OwnershipEvent.Received, pokemon.Ownership.Event);
    Assert.Equal(Catalog.Red.Id, pokemon.Ownership.TrainerId);
    Assert.Equal(Catalog.MasterBall.Id, pokemon.Ownership.PokeBallId);
    Assert.Equal(Catalog.PalletTown, pokemon.Ownership.MetAt);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
    PokemonReceived @event = pokemon.LastChange<PokemonReceived>();
    Assert.Equal(Catalog.Red.Id, @event.TrainerId);
  }

  [Fact(DisplayName = "It should receive an egg without setting the original trainer.")]
  public void Given_Egg_When_Receive_Then_OriginalTrainerNotSet()
  {
    Specimen pokemon = Catalog.CreatePokemon("egg", eggCycles: 5);

    pokemon.Receive(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown);

    Assert.NotNull(pokemon.Ownership);
    Assert.Null(pokemon.OriginalTrainerId);
  }

  [Fact(DisplayName = "It should transfer a Pokémon to another trainer and keep the original trainer.")]
  public void Given_OwnedPokemon_When_ReceiveOtherTrainer_Then_Transferred()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    pokemon.Receive(Catalog.Blue, Catalog.MasterBall, Catalog.CeruleanCity);

    Assert.Equal(Catalog.Blue.Id, pokemon.Ownership!.TrainerId);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
    Assert.Equal(Catalog.MasterBall.Id, pokemon.Ownership.PokeBallId);
    Assert.Equal(Catalog.CeruleanCity, pokemon.Ownership.MetAt);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyOwnedException when receiving the same trainer.")]
  public void Given_SameTrainer_When_Receive_Then_PokemonAlreadyOwnedException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    PokemonAlreadyOwnedException exception = Assert.Throws<PokemonAlreadyOwnedException>(
      () => pokemon.Receive(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown));
    Assert.Equal(Catalog.World.Id.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when transferring with a different Poké Ball.")]
  public void Given_DifferentPokeBall_When_Receive_Then_ImmutablePropertyException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, pokeBall: Catalog.MasterBall);

    ImmutablePropertyException<Guid> exception = Assert.Throws<ImmutablePropertyException<Guid>>(
      () => pokemon.Receive(Catalog.Blue, Catalog.PokeBall, Catalog.CeruleanCity));
    Assert.Equal(Specimen.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(pokemon.EntityId, exception.Data["EntityId"]);
    Assert.Equal(Catalog.MasterBall.EntityId, exception.Data["ExpectedValue"]);
    Assert.Equal(Catalog.PokeBall.EntityId, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidItemCategoryException when the item is not a Poké Ball.")]
  public void Given_NotPokeBall_When_Receive_Then_InvalidItemCategoryException()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    InvalidItemCategoryException exception = Assert.Throws<InvalidItemCategoryException>(
      () => pokemon.Receive(Catalog.Red, Catalog.Potion, Catalog.PalletTown));
    Assert.Equal(ItemCategory.PokeBall, exception.Data["ExpectedCategory"]);
    Assert.Equal(ItemCategory.Medicine, exception.Data["AttemptedCategory"]);
  }

  [Fact(DisplayName = "It should catch a wild Pokémon and set the original trainer.")]
  public void Given_WildPokemon_When_Catch_Then_Caught()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    pokemon.Catch(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown);

    Assert.Equal(OwnershipEvent.Caught, pokemon.Ownership!.Event);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
    Assert.IsType<PokemonCaught>(pokemon.LastChange<PokemonCaught>());
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotBeCaughtException when catching an egg.")]
  public void Given_Egg_When_Catch_Then_PokemonEggCannotBeCaughtException()
  {
    Specimen pokemon = Catalog.CreatePokemon(eggCycles: 5);

    PokemonEggCannotBeCaughtException exception = Assert.Throws<PokemonEggCannotBeCaughtException>(
      () => pokemon.Catch(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown));
    Assert.Equal(pokemon.EggCycles, exception.Data["EggCycles"]);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyOwnedException when catching an owned Pokémon.")]
  public void Given_OwnedPokemon_When_Catch_Then_PokemonAlreadyOwnedException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    Assert.Throws<PokemonAlreadyOwnedException>(() => pokemon.Catch(Catalog.Red, Catalog.MasterBall, Catalog.PalletTown));
  }

  [Fact(DisplayName = "It should release a Pokémon and keep the original trainer.")]
  public void Given_OwnedPokemon_When_Release_Then_OwnershipCleared()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    pokemon.Release();

    Assert.Null(pokemon.Ownership);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
    Assert.IsType<PokemonReleased>(pokemon.LastChange<PokemonReleased>());
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when releasing a wild Pokémon.")]
  public void Given_WildPokemon_When_Release_Then_PokemonHasNoOwnerException()
  {
    Specimen pokemon = Catalog.CreatePokemon();

    PokemonHasNoOwnerException exception = Assert.Throws<PokemonHasNoOwnerException>(
      () => pokemon.Release());
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotBeReleasedException when releasing an egg.")]
  public void Given_Egg_When_Release_Then_PokemonEggCannotBeReleasedException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, eggCycles: 5);

    Assert.Throws<PokemonEggCannotBeReleasedException>(() => pokemon.Release());
    Assert.NotNull(pokemon.Ownership);
  }

  [Fact(DisplayName = "It should catch a Pokémon after it was released.")]
  public void Given_ReleasedPokemon_When_Catch_Then_Caught()
  {
    Specimen pokemon = Catalog.CreateCaughtPokemon(Catalog.Red);
    pokemon.Release();

    pokemon.Catch(Catalog.Blue, Catalog.PokeBall, Catalog.CeruleanCity);

    Assert.Equal(OwnershipEvent.Caught, pokemon.Ownership!.Event);
    Assert.Equal(Catalog.Blue.Id, pokemon.Ownership.TrainerId);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
  }
}
