using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Rosters;

public class RosterDepositTests : UnitTests
{
  [Fact(DisplayName = "It should deposit a party Pokémon into the box.")]
  public void Given_PartyPokemon_When_Deposit_Then_MovedToBox()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "party");
    roster.Add(pokemon, Catalog.Red);

    roster.Deposit(pokemon);

    Assert.False(roster.Entries[pokemon.Id].IsInParty);
    Assert.DoesNotContain(pokemon.Id, roster.PartyIds);
    RosterEntryDeposited @event = roster.LastChange<RosterEntryDeposited>();
    Assert.Equal(pokemon.Id, @event.PokemonId);
  }

  [Fact(DisplayName = "It should throw PokemonNotInPartyException when depositing a boxed Pokémon.")]
  public void Given_BoxedPokemon_When_Deposit_Then_PokemonNotInPartyException()
  {
    Roster roster = FillParty();
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);

    PokemonNotInPartyException exception = Assert.Throws<PokemonNotInPartyException>(() => roster.Deposit(boxed));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(boxed.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonNotInRosterException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Deposit_Then_PokemonNotInRosterException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    PokemonNotInRosterException exception = Assert.Throws<PokemonNotInRosterException>(() => roster.Deposit(pokemon));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonOwnerException when the Pokémon belongs to another trainer.")]
  public void Given_OtherTrainer_When_Deposit_Then_InvalidPokemonOwnerException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Blue);

    InvalidPokemonOwnerException exception = Assert.Throws<InvalidPokemonOwnerException>(() => roster.Deposit(pokemon));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["ExpectedTrainerId"]);
    Assert.Equal(Catalog.Blue.EntityId, exception.Data["AttemptedTrainerId"]);
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the Pokémon belongs to another world.")]
  public void Given_DifferentWorld_When_Deposit_Then_WorldMismatchException()
  {
    Roster roster = new(Catalog.Red);
    DomainCatalog other = new(Faker);

    Assert.Throws<WorldMismatchException>(() => roster.Deposit(other.CreateOwnedPokemon(other.Red)));
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
