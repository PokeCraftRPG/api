using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Rosters;

[Trait(Traits.Category, Categories.Unit)]
public class RosterTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;
  private readonly Location _location = new("Pallet Town");
  private readonly Roster _roster;
  private readonly Specimen _specimen;

  public RosterTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
    _roster = new Roster(_trainer);
    _specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
  }

  [Fact(DisplayName = "Add: it should add the Pokémon to the roster.")]
  public void Given_Specimen_When_Add_Then_Added()
  {
    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);

    _roster.Add(_specimen, _world.OwnerId);
    Assert.Equal(new PokemonSlot(0), _specimen.Slot);
    Assert.Equal([_specimen.Id], _roster.GetParty());
    Assert.True(_roster.HasChanges);
    Assert.Contains(_roster.Changes, change => change is RosterPokemonMoved moved && moved.ActorId == _world.OwnerId.ActorId);

    _roster.ClearChanges();
    _roster.Add(_specimen, _world.OwnerId);
    Assert.False(_roster.HasChanges);
    Assert.Empty(_roster.Changes);
  }

  [Theory(DisplayName = "Add: it should throw ArgumentException when the ownership is not valid.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_InvalidOwnership_When_Add_Then_ArgumentException(bool withOwnership)
  {
    Trainer? trainer = withOwnership ? new TrainerBuilder().WithWorld(_world).Build() : null;
    if (trainer is not null)
    {
      _specimen.Receive(trainer, _pokeBall, _location, _world.OwnerId);
    }

    var exception = Assert.Throws<ArgumentException>(() => _roster.Add(_specimen, _world.OwnerId));
    Assert.Equal("specimen", exception.ParamName);

    string message = trainer is null
      ? $"The Pokémon current trainer 'Id=<null>' must be '{_trainer.Id}'."
      : $"The Pokémon current trainer 'Id={trainer.Id}' must be '{_trainer.Id}'.";
    Assert.StartsWith(message, exception.Message);
  }

  [Fact(DisplayName = "Add: it should throw RosterIsFullException when the roster is full.")]
  public void Given_RosterIsFull_When_Add_Then_RosterIsFullException()
  {
    int limit = PokemonSlot.PartySize + (PokemonSlot.BoxCount * PokemonSlot.BoxSize);
    for (int i = 0; i < limit; i++)
    {
      Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
      specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
      _roster.Add(specimen, _world.OwnerId);
    }

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    var exception = Assert.Throws<RosterIsFullException>(() => _roster.Add(_specimen, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
  }

  [Fact(DisplayName = "Add: it should throw WorldMismatchException when the Pokémon is not in the same world.")]
  public void Given_WorldMismatch_When_Add_Then_WorldMismatchException()
  {
    Specimen specimen = new SpecimenBuilder().Build();

    var exception = Assert.Throws<WorldMismatchException>(() => _roster.Add(specimen, _world.OwnerId));
    Assert.Equal(_roster.Id.GetEntity(), exception.Expected);
    Assert.Equal(specimen.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "Deposit: it should deposit the Pokémon in the first available boxed slot.")]
  public void Given_NotOnlyInParty_When_Deposit_Then_Deposited()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(specimen, _world.OwnerId);

    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    PokemonParty party = new([specimen, _specimen]);
    _roster.Deposit(_specimen, party, _world.OwnerId);

    Assert.Equal(new PokemonSlot(0, 0), _specimen.Slot);
    Assert.Contains(_specimen.Changes, change => change is PokemonDeposited deposited && deposited.ActorId == _world.OwnerId.ActorId);

    Assert.Equal([specimen.Id], _roster.GetParty());
  }

  [Fact(DisplayName = "Deposit: it should throw ArgumentException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Deposit_Then_ArgumentException()
  {
    PokemonParty party = new(_trainer.Id);
    var exception = Assert.Throws<ArgumentException>(() => _roster.Deposit(_specimen, party, _world.OwnerId));
    Assert.Equal("specimen", exception.ParamName);
    Assert.StartsWith($"The Pokémon '{_specimen}' is not in the trainer 'Id={_trainer.Id}' roster.", exception.Message);
  }

  [Fact(DisplayName = "Deposit: it should throw InvalidPartyException when the Pokémon is the only one in the party.")]
  public void Given_OnlyInParty_When_Deposit_Then_InvalidPartyException()
  {
    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(egg, _world.OwnerId);

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    PokemonParty party = new([egg, _specimen]);
    var exception = Assert.Throws<InvalidPartyException>(() => _roster.Deposit(_specimen, party, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.Equal([egg.EntityId, _specimen.EntityId], exception.MemberIds);
  }

  [Fact(DisplayName = "Deposit: it should throw PokemonIsNotInPartyException when the Pokémon is not in the party.")]
  public void Given_NotInParty_When_Deposit_Then_PokemonIsNotInPartyException()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(specimen, _world.OwnerId);

    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    PokemonParty party = new([specimen, _specimen]);
    _roster.Deposit(_specimen, party, _world.OwnerId);

    var exception = Assert.Throws<PokemonIsNotInPartyException>(() => _roster.Deposit(_specimen, party, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
  }

  [Fact(DisplayName = "Deposit: it should throw RosterIsFullException when the boxes are full.")]
  public void Given_BoxesAreFull_When_Deposit_Then_RosterIsFullException()
  {
    List<Specimen> members = [_specimen];

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    for (int i = 1; i < PokemonSlot.PartySize; i++)
    {
      Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
      members.Add(specimen);
      _roster.Add(specimen, _world.OwnerId);
    }

    int total = PokemonSlot.BoxCount * PokemonSlot.BoxSize;
    for (int i = 0; i < total; i++)
    {
      Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
      _roster.Add(specimen, _world.OwnerId);
    }

    PokemonParty party = new(members);
    var exception = Assert.Throws<RosterIsFullException>(() => _roster.Deposit(_specimen, party, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
  }

  [Fact(DisplayName = "Release: it should release the Pokémon when it is not the only one in the party.")]
  public void Given_NotOnlyInParty_When_Release_Then_Released()
  {
    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(specimen, _world.OwnerId);
    Assert.Equal(new PokemonSlot(1), specimen.Slot);

    PokemonParty party = new([_specimen, specimen]);
    _roster.Release(_specimen, party, _world.OwnerId);

    Assert.Null(_specimen.Ownership);
    Assert.Equal(new PokemonSlot(0), specimen.Slot);
    Assert.Equal([specimen.Id], _roster.GetParty());
  }

  [Fact(DisplayName = "Release: it should throw ArgumentException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Release_Then_ArgumentException()
  {
    PokemonParty party = new(_trainer.Id);
    var exception = Assert.Throws<ArgumentException>(() => _roster.Release(_specimen, party, _world.OwnerId));
    Assert.Equal("specimen", exception.ParamName);
    Assert.StartsWith($"The Pokémon '{_specimen}' is not in the trainer 'Id={_trainer.Id}' roster.", exception.Message);
  }

  [Fact(DisplayName = "Release: it should throw InvalidPartyException when the Pokémon is the only one in the party.")]
  public void Given_OnlyInParty_When_Release_Then_InvalidPartyException()
  {
    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(egg, _world.OwnerId);

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    PokemonParty party = new([egg, _specimen]);
    var exception = Assert.Throws<InvalidPartyException>(() => _roster.Release(_specimen, party, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.Equal([egg.EntityId, _specimen.EntityId], exception.MemberIds);
  }

  [Fact(DisplayName = "Withdraw: it should withdraw the Pokémon in the first available party slot.")]
  public void Given_InBoxes_When_Withdraw_Then_Withdrawn()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(specimen, _world.OwnerId);

    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    PokemonParty party = new([specimen, _specimen]);
    _roster.Deposit(_specimen, party, _world.OwnerId);

    _roster.Withdraw(_specimen, _world.OwnerId);

    Assert.Equal(new PokemonSlot(1), _specimen.Slot);
    Assert.Contains(_specimen.Changes, change => change is PokemonWithdrawn withdrawn && withdrawn.ActorId == _world.OwnerId.ActorId);

    Assert.Equal([specimen.Id, _specimen.Id], _roster.GetParty());
  }

  [Fact(DisplayName = "Withdraw: it should throw ArgumentException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Withdraw_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => _roster.Withdraw(_specimen, _world.OwnerId));
    Assert.Equal("specimen", exception.ParamName);
    Assert.StartsWith($"The Pokémon '{_specimen}' is not in the trainer 'Id={_trainer.Id}' roster.", exception.Message);
  }

  [Fact(DisplayName = "Withdraw: it should throw PartyIsFullException when the boxes are full.")]
  public void Given_PartyIsFull_When_Withdraw_Then_PartyIsFullException()
  {
    for (int i = 0; i < PokemonSlot.PartySize; i++)
    {
      Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
      _roster.Add(specimen, _world.OwnerId);
    }

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    var exception = Assert.Throws<PartyIsFullException>(() => _roster.Withdraw(_specimen, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
  }

  [Fact(DisplayName = "Withdraw: it should throw PokemonAlreadyInPartyException when the Pokémon is already in the party.")]
  public void Given_AlreadyInParty_When_Withdraw_Then_PokemonAlreadyInPartyException()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    _roster.Add(specimen, _world.OwnerId);

    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);
    _roster.Add(_specimen, _world.OwnerId);

    var exception = Assert.Throws<PokemonAlreadyInPartyException>(() => _roster.Withdraw(_specimen, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
  }
}
