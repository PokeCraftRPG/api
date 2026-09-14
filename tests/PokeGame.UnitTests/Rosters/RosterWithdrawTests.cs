using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Rosters;

public class RosterWithdrawTests : UnitTests
{
  [Fact(DisplayName = "It should withdraw a boxed Pokémon into the party.")]
  public void Given_BoxedPokemonWithRoom_When_Withdraw_Then_MovedToParty()
  {
    Roster roster = new(Catalog.Red);
    Specimen party = Catalog.CreateOwnedPokemon(Catalog.Red, "party");
    roster.Add(party, Catalog.Red);
    for (int index = 1; index < Roster.PartyLimit; index++)
    {
      roster.Add(Catalog.CreateOwnedPokemon(Catalog.Red, $"party-{index}"), Catalog.Red);
    }
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);
    roster.Deposit(party);

    roster.Withdraw(boxed, Catalog.Red);

    Assert.True(roster.Entries[boxed.Id].IsInParty);
    Assert.Contains(boxed.Id, roster.PartyIds);
    RosterEntryWithdrawn @event = roster.LastChange<RosterEntryWithdrawn>();
    Assert.Equal(boxed.Id, @event.PokemonId);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyInPartyException when withdrawing a party Pokémon.")]
  public void Given_PartyPokemon_When_Withdraw_Then_PokemonAlreadyInPartyException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "party");
    roster.Add(pokemon, Catalog.Red);

    PokemonAlreadyInPartyException exception = Assert.Throws<PokemonAlreadyInPartyException>(
      () => roster.Withdraw(pokemon, Catalog.Red));
    Assert.Equal(pokemon.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotBeWithdrawnException when withdrawing an egg.")]
  public void Given_Egg_When_Withdraw_Then_PokemonEggCannotBeWithdrawnException()
  {
    Roster roster = FillParty();
    Specimen egg = Catalog.CreateOwnedPokemon(Catalog.Red, "egg", eggCycles: 5);
    roster.Add(egg, Catalog.Red);

    PokemonEggCannotBeWithdrawnException exception = Assert.Throws<PokemonEggCannotBeWithdrawnException>(
      () => roster.Withdraw(egg, Catalog.Red));
    Assert.Equal(egg.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(egg.EntityId, exception.Data["PokemonId"]);
    Assert.Equal((byte)5, exception.Data["EggCycles"]);
  }

  [Fact(DisplayName = "It should throw PokemonPartyFullException when the party is full.")]
  public void Given_PartyIsFull_When_Withdraw_Then_PokemonPartyFullException()
  {
    Roster roster = FillParty();
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);

    PokemonPartyFullException exception = Assert.Throws<PokemonPartyFullException>(
      () => roster.Withdraw(boxed, Catalog.Red));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(Roster.PartyLimit, exception.Data["PartyLimit"]);
    Assert.Equal(Roster.PartyLimit, exception.Data["PartyCount"]);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Withdraw_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Withdraw(pokemon, Catalog.Red));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon belongs to another trainer.")]
  public void Given_OtherTrainerPokemon_When_Withdraw_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Blue);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Withdraw(pokemon, Catalog.Red));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the trainer does not match the roster.")]
  public void Given_DifferentTrainer_When_Withdraw_Then_ArgumentException()
  {
    Roster roster = FillParty();
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Withdraw(boxed, Catalog.Blue));
    Assert.Equal("trainer", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the Pokémon belongs to another world.")]
  public void Given_DifferentWorld_When_Withdraw_Then_WorldMismatchException()
  {
    Roster roster = new(Catalog.Red);
    DomainCatalog other = new(Faker);

    Assert.Throws<WorldMismatchException>(() => roster.Withdraw(other.CreateOwnedPokemon(other.Red), Catalog.Red));
  }

  private Roster FillParty()
  {
    Roster roster = new(Catalog.Red);
    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      roster.Add(Catalog.CreateOwnedPokemon(Catalog.Red, $"party-{index}"), Catalog.Red);
    }
    return roster;
  }
}
