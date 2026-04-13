using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class SpecimenOwnershipTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Specimen _specimen;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;
  private readonly Location _location = new("Viridian Forest");

  public SpecimenOwnershipTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
  }

  [Fact(DisplayName = "Catch: it should catch a wild Pokémon.")]
  public void Given_Wild_When_Catch_Then_Caught()
  {
    Assert.False(_specimen.OriginalTrainerId.HasValue);
    Assert.Null(_specimen.Ownership);

    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(_location, _specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonCaught caught && caught.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Catch: it should throw CannotCatchOwnedPokemonException when the Pokémon already has a trainer.")]
  public void Given_NotWild_When_Catch_Then_CannotCatchOwnedPokemonException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);

    var exception = Assert.Throws<CannotCatchOwnedPokemonException>(() => _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
  }

  [Fact(DisplayName = "Catch: it should throw InvalidItemException when the item is not a Poké Ball.")]
  public void Given_ItemNotPokeBall_When_Catch_Then_InvalidItemException()
  {
    Item potion = ItemBuilder.Potion(_faker, _world);

    var exception = Assert.Throws<InvalidItemException>(() => _specimen.Catch(_trainer, potion, _location, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(potion.EntityId, exception.ItemId);
    Assert.Equal(potion.Category, exception.ActualCategory);
    Assert.Equal(ItemCategory.PokeBall, exception.ExpectedCategory);
    Assert.Equal("PokeBallId", exception.PropertyName);
  }

  [Fact(DisplayName = "Catch: it should throw WorldMismatchException when the Poké Ball is not in the same world.")]
  public void Given_PokeBallWorldMismatch_When_Catch_Then_WorldMismatchException()
  {
    Item pokeBall = ItemBuilder.PokeBall();

    var exception = Assert.Throws<WorldMismatchException>(() => _specimen.Catch(_trainer, pokeBall, _location, _world.OwnerId));
    Assert.Equal(_specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(pokeBall.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("pokeBall", exception.ParamName);
  }

  [Fact(DisplayName = "Catch: it should throw WorldMismatchException when the trainer is not in the same world.")]
  public void Given_TrainerWorldMismatch_When_Catch_Then_WorldMismatchException()
  {
    Trainer trainer = new TrainerBuilder().Build();

    var exception = Assert.Throws<WorldMismatchException>(() => _specimen.Catch(trainer, _pokeBall, _location, _world.OwnerId));
    Assert.Equal(_specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(trainer.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("trainer", exception.ParamName);
  }

  [Fact(DisplayName = "Receive: it should receive a Pokémon.")]
  public void Given_Wild_When_Receive_Then_Caught()
  {
    Assert.False(_specimen.OriginalTrainerId.HasValue);
    Assert.Null(_specimen.Ownership);

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(_location, _specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonReceived received && received.ActorId == _world.OwnerId.ActorId);

    _specimen.ClearChanges();

    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    Item greatBall = new ItemBuilder(_faker).WithWorld(_world).WithKey(new Slug("great-ball")).IsPokeBall(new PokeBallProperties(1.5, false, 0, 1.0)).Build();
    Location location = new("Mt. Coronet");

    _specimen.Receive(trainer, greatBall, location, _world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(greatBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(location, _specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonReceived received && received.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Receive: it should throw InvalidItemException when the item is not a Poké Ball.")]
  public void Given_ItemNotPokeBall_When_Receive_Then_InvalidItemException()
  {
    Item potion = ItemBuilder.Potion(_faker, _world);

    var exception = Assert.Throws<InvalidItemException>(() => _specimen.Receive(_trainer, potion, _location, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(potion.EntityId, exception.ItemId);
    Assert.Equal(potion.Category, exception.ActualCategory);
    Assert.Equal(ItemCategory.PokeBall, exception.ExpectedCategory);
    Assert.Equal("PokeBallId", exception.PropertyName);
  }

  [Fact(DisplayName = "Receive: it should throw WorldMismatchException when the Poké Ball is not in the same world.")]
  public void Given_PokeBallWorldMismatch_When_Receive_Then_WorldMismatchException()
  {
    Item pokeBall = ItemBuilder.PokeBall();

    var exception = Assert.Throws<WorldMismatchException>(() => _specimen.Receive(_trainer, pokeBall, _location, _world.OwnerId));
    Assert.Equal(_specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(pokeBall.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("pokeBall", exception.ParamName);
  }

  [Fact(DisplayName = "Receive: it should throw WorldMismatchException when the trainer is not in the same world.")]
  public void Given_TrainerWorldMismatch_When_Receive_Then_WorldMismatchException()
  {
    Trainer trainer = new TrainerBuilder().Build();

    var exception = Assert.Throws<WorldMismatchException>(() => _specimen.Receive(trainer, _pokeBall, _location, _world.OwnerId));
    Assert.Equal(_specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(trainer.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("trainer", exception.ParamName);
  }

  [Fact(DisplayName = "Release: it should release a Pokémon owned by a trainer.")]
  public void Given_OwnedPokemon_When_Release_Then_Released()
  {
    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.True(_specimen.OriginalTrainerId.HasValue);
    Assert.NotNull(_specimen.Ownership);

    _specimen.Release(_world.OwnerId);
    Assert.False(_specimen.OriginalTrainerId.HasValue);
    Assert.Null(_specimen.Ownership);
    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonReleased released && released.ActorId == _world.OwnerId.ActorId);

    _specimen.ClearChanges();
    _specimen.Release(_world.OwnerId);
    Assert.False(_specimen.HasChanges);
    Assert.Empty(_specimen.Changes);
  }

  [Fact(DisplayName = "Release: it should throw CannotReleaseEggPokemonException when the Pokémon is still an egg.")]
  public void Given_EggPokemon_When_Release_Then_CannotReleaseEggPokemonException()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Build();
    specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    var exception = Assert.Throws<CannotReleaseEggPokemonException>(() => specimen.Release(_world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(specimen.EntityId, exception.PokemonId);
    Assert.Equal(specimen.EggCycles?.Value, exception.EggCycles);
  }
}
