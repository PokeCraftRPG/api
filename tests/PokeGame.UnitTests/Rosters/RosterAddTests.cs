using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Rosters;

public class RosterAddTests : UnitTests
{
  [Fact(DisplayName = "It should add a Pokémon to the party when a slot is available.")]
  public void Given_PartyHasRoom_When_Add_Then_AddedToParty()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "party");

    roster.Add(pokemon, Catalog.Red);

    Assert.True(roster.Entries[pokemon.Id].IsInParty);
    Assert.Equal(0, roster.Entries[pokemon.Id].Priority);
    Assert.Contains(pokemon.Id, roster.PartyIds);
    RosterEntryAdded @event = roster.LastChange<RosterEntryAdded>();
    Assert.Equal(pokemon.Id, @event.PokemonId);
    Assert.True(@event.IsInParty);
  }

  [Fact(DisplayName = "It should add a Pokémon to the box when the party is full.")]
  public void Given_PartyIsFull_When_Add_Then_AddedToBox()
  {
    Roster roster = FillParty(Catalog.Red);
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");

    roster.Add(boxed, Catalog.Red);

    Assert.False(roster.Entries[boxed.Id].IsInParty);
    Assert.DoesNotContain(boxed.Id, roster.PartyIds);
    Assert.Equal(Roster.PartyLimit, roster.PartyIds.Count);
    Assert.False(roster.LastChange<RosterEntryAdded>().IsInParty);
  }

  [Fact(DisplayName = "It should respect a custom trainer party limit.")]
  public void Given_CustomPartyLimit_When_Add_Then_BoxedWhenLimitReached()
  {
    SetPartyLimit(Catalog.Red, 1);
    Roster roster = new(Catalog.Red);
    Specimen first = Catalog.CreateOwnedPokemon(Catalog.Red, "first");
    Specimen second = Catalog.CreateOwnedPokemon(Catalog.Red, "second");

    roster.Add(first, Catalog.Red);
    roster.Add(second, Catalog.Red);

    Assert.True(roster.Entries[first.Id].IsInParty);
    Assert.False(roster.Entries[second.Id].IsInParty);
    Assert.Single(roster.PartyIds);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is already in the roster.")]
  public void Given_AlreadyInRoster_When_Add_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    roster.Add(pokemon, Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Add(pokemon, Catalog.Red));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is not owned by the trainer.")]
  public void Given_NotOwned_When_Add_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreatePokemon("wild");

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Add(pokemon, Catalog.Red));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is owned by another trainer.")]
  public void Given_OwnedByOtherTrainer_When_Add_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Blue, "blue");

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Add(pokemon, Catalog.Red));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the trainer does not match the roster.")]
  public void Given_DifferentTrainer_When_Add_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Add(pokemon, Catalog.Blue));
    Assert.Equal("trainer", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the Pokémon belongs to another world.")]
  public void Given_DifferentWorld_When_Add_Then_WorldMismatchException()
  {
    Roster roster = new(Catalog.Red);
    DomainCatalog other = new(Faker);

    Assert.Throws<WorldMismatchException>(() => roster.Add(other.CreateOwnedPokemon(other.Red), Catalog.Red));
  }

  private Roster FillParty(Trainer trainer)
  {
    Roster roster = new(trainer);
    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      Specimen pokemon = Catalog.CreateOwnedPokemon(trainer, $"party-{index}");
      roster.Add(pokemon, trainer);
    }
    return roster;
  }

  private static void SetPartyLimit(Trainer trainer, int partyLimit)
  {
    typeof(Trainer).GetProperty(nameof(Trainer.PartyLimit))!.SetValue(trainer, partyLimit);
  }
}
