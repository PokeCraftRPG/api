using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class PokemonPartyTests
{
  private readonly Faker _faker = new();
  private readonly World _world;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;
  private readonly Location _location = new("Pallet Town");

  public PokemonPartyTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
  }

  [Fact(DisplayName = "EnsureIsValidWithout: it should not do anything when there are other battle-ready Pokémon.")]
  public void Given_OtherBattleReady_When_EnsureIsValidWithout_Then_DoNothing()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen.Move(new PokemonSlot(0), _world.OwnerId);

    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();

    PokemonParty party = new([specimen]);
    party.EnsureIsValidWithout(egg);
  }

  [Fact(DisplayName = "EnsureIsValidWithout: it should throw InvalidPartyException when there is no other battle-ready Pokémon.")]
  public void Given_NoOtherBattleReady_When_EnsureIsValidWithout_Then_False()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen.Move(new PokemonSlot(0), _world.OwnerId);

    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();
    egg.Move(new PokemonSlot(1), _world.OwnerId);

    Specimen unconscious = new SpecimenBuilder(_faker).WithWorld(_world).WithStamina(0).Received(_trainer, _pokeBall, _location).Build();
    unconscious.Move(new PokemonSlot(2), _world.OwnerId);

    PokemonParty party = new([specimen, egg, unconscious]);
    var exception = Assert.Throws<InvalidPartyException>(() => party.EnsureIsValidWithout(specimen));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
    Assert.True(new Guid[] { specimen.EntityId, egg.EntityId, unconscious.EntityId }.SequenceEqual(exception.MemberIds));
  }

  [Fact(DisplayName = "IsValidWithout: it should return false when there is no other battle-ready Pokémon.")]
  public void Given_NoOtherBattleReady_When_IsValidWithout_Then_False()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen.Move(new PokemonSlot(0), _world.OwnerId);

    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();
    egg.Move(new PokemonSlot(1), _world.OwnerId);

    Specimen unconscious = new SpecimenBuilder(_faker).WithWorld(_world).WithStamina(0).Received(_trainer, _pokeBall, _location).Build();
    unconscious.Move(new PokemonSlot(2), _world.OwnerId);

    PokemonParty party = new([specimen, egg, unconscious]);
    Assert.False(party.IsValidWithout(specimen));
  }

  [Fact(DisplayName = "IsValidWithout: it should return true when there are other battle-ready Pokémon.")]
  public void Given_OtherBattleReady_When_IsValidWithout_Then_True()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen.Move(new PokemonSlot(0), _world.OwnerId);

    Specimen egg = new SpecimenBuilder(_faker).WithWorld(_world).IsEgg().Received(_trainer, _pokeBall, _location).Build();

    PokemonParty party = new([specimen]);
    Assert.True(party.IsValidWithout(egg));
  }

  [Fact(DisplayName = "It should construct an instance from a trainer ID.")]
  public void Given_TrainerId_When_ctor_Then_Party()
  {
    Trainer trainer = new TrainerBuilder(_faker).Build();
    PokemonParty party = new(trainer.Id);
    Assert.Equal(trainer.Id, party.TrainerId);
    Assert.True(party.IsEmpty);
    Assert.Empty(party.Members);
  }

  [Fact(DisplayName = "It should construct an instance from members.")]
  public void Given_Members_When_ctor_Then_Party()
  {
    Specimen specimen1 = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    Specimen specimen2 = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();

    Roster roster = new(_trainer);
    roster.Add(specimen1, _world.OwnerId);
    roster.Add(specimen2, _world.OwnerId);

    PokemonParty party = new([specimen1, specimen2]);
    Assert.Equal(_trainer.Id, party.TrainerId);
    Assert.False(party.IsEmpty);
    Assert.Equal(specimen1, party.Members.ElementAt(0));
    Assert.Equal(specimen2, party.Members.ElementAt(1));
  }

  [Fact(DisplayName = "It should throw ArgumentException when the party is not valid.")]
  public void Given_Invalid_When_ctor_Then_ArgumentException()
  {
    Specimen valid = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    Specimen wild = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    Specimen ownedWithoutSlot = new SpecimenBuilder(_faker).WithWorld(_world).Caught(_trainer, _pokeBall, _location).Build();
    Specimen boxed = new SpecimenBuilder(_faker).WithWorld(_world).Caught(_trainer, _pokeBall, _location).Build();
    Specimen conflict1 = new SpecimenBuilder(_faker).WithWorld(_world).Caught(_trainer, _pokeBall, _location).Build();
    Specimen conflict2 = new SpecimenBuilder(_faker).WithWorld(_world).Caught(_trainer, _pokeBall, _location).Build();

    Roster roster = new(_trainer);
    roster.Add(valid, _world.OwnerId);
    roster.Add(boxed, _world.OwnerId);
    roster.Deposit(boxed, new PokemonParty([valid, boxed]), _world.OwnerId);

    PokemonSlot slot = new(1);
    conflict1.Move(slot, _world.OwnerId);
    conflict2.Move(slot, _world.OwnerId);

    Specimen[] members = [valid, wild, ownedWithoutSlot, boxed, conflict1, conflict2];
    var exception = Assert.Throws<ArgumentException>(() => new PokemonParty(members));
    Assert.StartsWith("The Pokémon must have an owner and a party slot, and each slot may only contain 1 Pokémon.", exception.Message);
    Assert.DoesNotContain(valid.ToString(), exception.Message);
    Assert.Contains(wild.ToString(), exception.Message);
    Assert.Contains(ownedWithoutSlot.ToString(), exception.Message);
    Assert.Contains(boxed.ToString(), exception.Message);
    Assert.Contains(conflict1.ToString(), exception.Message);
    Assert.Contains(conflict2.ToString(), exception.Message);
    Assert.Equal("specimens", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when there are multiple trainers.")]
  public void Given_MultipleTrainers_When_ctor_Then_ArgumentException()
  {
    Specimen specimen1 = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen1.Move(new PokemonSlot(0), _world.OwnerId);

    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    Specimen specimen2 = new SpecimenBuilder(_faker).WithWorld(_world).Received(trainer, _pokeBall, _location).Build();
    specimen2.Move(new PokemonSlot(1), _world.OwnerId);

    Specimen[] members = [specimen1, specimen2];
    var exception = Assert.Throws<ArgumentException>(() => new PokemonParty(members));
    Assert.StartsWith("All party members must be owned by the same trainer.", exception.Message);
    Assert.Equal("specimens", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when there is a missing Pokémon.")]
  public void Given_MissingPokemon_When_ctor_Then_ArgumentException()
  {
    Specimen specimen1 = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen1.Move(new PokemonSlot(0), _world.OwnerId);

    Specimen specimen2 = new SpecimenBuilder(_faker).WithWorld(_world).Received(_trainer, _pokeBall, _location).Build();
    specimen2.Move(new PokemonSlot(2), _world.OwnerId);

    Specimen[] members = [specimen1, specimen2];
    var exception = Assert.Throws<ArgumentException>(() => new PokemonParty(members));
    Assert.StartsWith("The party is missing a Pokémon at position 1.", exception.Message);
    Assert.Equal("specimens", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when there is no member.")]
  public void Given_NoMember_When_ctor_Then_ArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => new PokemonParty([]));
    Assert.StartsWith("At least one Pokémon must be provided.", exception.Message);
    Assert.Equal("specimens", exception.ParamName);
  }
}
