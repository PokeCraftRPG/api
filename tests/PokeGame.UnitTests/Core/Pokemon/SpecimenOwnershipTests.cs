using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
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
    Assert.Equal(OwnershipKind.Caught, _specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(_location, _specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonCaught caught && caught.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Catch: it should catch an egg Pokémon.")]
  public void Given_Egg_When_Catch_Then_Caught()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Build();
    Assert.False(specimen.OriginalTrainerId.HasValue);
    Assert.Null(specimen.Ownership);

    specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.Null(specimen.OriginalTrainerId);

    Assert.NotNull(specimen.Ownership);
    Assert.Equal(OwnershipKind.Caught, specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, specimen.Ownership.PokeBallId);
    Assert.Equal(specimen.Level, specimen.Ownership.Level.Value);
    Assert.Equal(_location, specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(specimen.HasChanges);
    Assert.Contains(specimen.Changes, change => change is PokemonCaught caught && caught.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Catch: it should throw PokemonIsNotWildException when the Pokémon already has a trainer.")]
  public void Given_NotWild_When_Catch_Then_PokemonIsNotWildException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);

    var exception = Assert.Throws<PokemonIsNotWildException>(() => _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId));
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

  [Fact(DisplayName = "Deposit: it should deposit a Pokémon in a box.")]
  public void Given_InParty_When_Deposit_Then_Deposited()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _specimen.Move(new PokemonSlot(0), _world.OwnerId);

    PokemonSlot slot = new(0, 0);
    _specimen.Deposit(slot, _world.OwnerId);
    Assert.Equal(slot, _specimen.Slot);
    Assert.Contains(_specimen.Changes, change => change is PokemonDeposited deposited && deposited.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Deposit: it should throw ArgumentException when the new slot is not in a box.")]
  public void Given_NotInBox_When_Deposit_Then_ArgumentException()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    PokemonSlot slot = new(0);
    _specimen.Move(slot, _world.OwnerId);

    var exception = Assert.Throws<ArgumentException>(() => _specimen.Deposit(slot, _world.OwnerId));
    Assert.Equal("slot", exception.ParamName);
    Assert.StartsWith("The slot must have a box number.", exception.Message);
  }

  [Fact(DisplayName = "Deposit: it should throw InvalidOperationException when the Pokémon has no owner.")]
  public void Given_HasNoOwner_When_Deposit_Then_InvalidOperationException()
  {
    var exception = Assert.Throws<InvalidOperationException>(() => _specimen.Deposit(new PokemonSlot(0, 0), _world.OwnerId));
    Assert.Equal($"The Pokémon 'Id={_specimen.Id}' is not owned by any trainer.", exception.Message);
  }

  [Fact(DisplayName = "Deposit: it should throw InvalidOperationException when the Pokémon is not in the party.")]
  public void Given_NotInParty_When_Deposit_Then_InvalidOperationException()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    PokemonSlot slot = new(0, 0);
    _specimen.Move(slot, _world.OwnerId);

    var exception = Assert.Throws<InvalidOperationException>(() => _specimen.Deposit(slot, _world.OwnerId));
    Assert.Equal($"The Pokémon 'Id={_specimen.Id}' is not in the party of its owning trainer.", exception.Message);
  }

  [Fact(DisplayName = "Move: it should move the Pokémon into the correct slot.")]
  public void Given_Ownership_When_Move_Then_Moved()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    PokemonSlot slot = new(1, 1);
    _specimen.Move(slot, _world.OwnerId);
    Assert.Equal(slot, _specimen.Slot);
    Assert.Contains(_specimen.Changes, change => change is PokemonMoved moved && moved.ActorId == _world.OwnerId.ActorId);

    _specimen.ClearChanges();
    _specimen.Move(slot, _world.OwnerId);
    Assert.False(_specimen.HasChanges);
    Assert.Empty(_specimen.Changes);
  }

  [Fact(DisplayName = "Move: it should throw InvalidOperationException when the Pokémon is not owned by any trainer.")]
  public void Given_NotOwned_When_Move_Then_InvalidOperationException()
  {
    var exception = Assert.Throws<InvalidOperationException>(() => _specimen.Move(new PokemonSlot(0), _world.OwnerId));
    Assert.Equal($"The Pokémon 'Id={_specimen.Id}' is not owned by any trainer.", exception.Message);
  }

  [Fact(DisplayName = "Receive: it should receive a wild Pokémon.")]
  public void Given_Wild_When_Receive_Then_Received()
  {
    Assert.False(_specimen.OriginalTrainerId.HasValue);
    Assert.Null(_specimen.Ownership);

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(OwnershipKind.Received, _specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(_location, _specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonReceived received && received.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Receive: it should receive an egg Pokémon.")]
  public void Given_Egg_When_Receive_Then_Received()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Build();
    Assert.False(specimen.OriginalTrainerId.HasValue);
    Assert.Null(specimen.Ownership);

    specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    Assert.Null(specimen.OriginalTrainerId);

    Assert.NotNull(specimen.Ownership);
    Assert.Equal(OwnershipKind.Received, specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, specimen.Ownership.PokeBallId);
    Assert.Equal(specimen.Level, specimen.Ownership.Level.Value);
    Assert.Equal(_location, specimen.Ownership.Location);
    Assert.Equal(DateTime.Now, specimen.Ownership.MetOn, TimeSpan.FromSeconds(1));

    Assert.True(specimen.HasChanges);
    Assert.Contains(specimen.Changes, change => change is PokemonReceived received && received.ActorId == _world.OwnerId.ActorId);
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

  [Fact(DisplayName = "Receive: it should throw PokemonIsNotWildException when the Pokémon already has a trainer.")]
  public void Given_NotWild_When_Receive_Then_PokemonIsNotWildException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    var exception = Assert.Throws<PokemonIsNotWildException>(() => _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
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
    _specimen.Move(new PokemonSlot(0), _world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);
    Assert.NotNull(_specimen.Ownership);
    Assert.NotNull(_specimen.Slot);

    _specimen.Release(_world.OwnerId);
    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);
    Assert.Null(_specimen.Ownership);
    Assert.Null(_specimen.Slot);
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

  [Fact(DisplayName = "Withdraw: it should withdraw a Pokémon from a box.")]
  public void Given_InParty_When_Withdraw_Then_Withdrawed()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _specimen.Move(new PokemonSlot(0, 0), _world.OwnerId);

    PokemonSlot slot = new(0);
    _specimen.Withdraw(slot, _world.OwnerId);
    Assert.Equal(slot, _specimen.Slot);
    Assert.Contains(_specimen.Changes, change => change is PokemonWithdrawn withdrawn && withdrawn.ActorId == _world.OwnerId.ActorId);
  }

  [Fact(DisplayName = "Withdraw: it should throw ArgumentException when the new slot is in a box.")]
  public void Given_NotInBox_When_Withdraw_Then_ArgumentException()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    PokemonSlot slot = new(0, 0);
    _specimen.Move(slot, _world.OwnerId);

    var exception = Assert.Throws<ArgumentException>(() => _specimen.Withdraw(slot, _world.OwnerId));
    Assert.Equal("slot", exception.ParamName);
    Assert.StartsWith("The slot must not have a box number.", exception.Message);
  }

  [Fact(DisplayName = "Withdraw: it should throw InvalidOperationException when the Pokémon has no owner.")]
  public void Given_HasNoOwner_When_Withdraw_Then_InvalidOperationException()
  {
    var exception = Assert.Throws<InvalidOperationException>(() => _specimen.Withdraw(new PokemonSlot(0, 0), _world.OwnerId));
    Assert.Equal($"The Pokémon 'Id={_specimen.Id}' is not owned by any trainer.", exception.Message);
  }

  [Fact(DisplayName = "Withdraw: it should throw InvalidOperationException when the Pokémon is already in the party.")]
  public void Given_NotInParty_When_Withdraw_Then_InvalidOperationException()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);

    PokemonSlot slot = new(0);
    _specimen.Move(slot, _world.OwnerId);

    var exception = Assert.Throws<InvalidOperationException>(() => _specimen.Withdraw(slot, _world.OwnerId));
    Assert.Equal($"The Pokémon 'Id={_specimen.Id}' is already in the party of its owning trainer.", exception.Message);
  }
}
