using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Rosters;

public class RosterSwapTests : UnitTests
{
  [Fact(DisplayName = "It should swap a party Pokémon with a boxed Pokémon.")]
  public void Given_PartyAndBox_When_Swap_Then_SlotsExchanged()
  {
    (Roster roster, Specimen party, Specimen boxed) = CreatePartyAndBoxed();
    int partyPriority = roster.Entries[party.Id].Priority;
    int boxedPriority = roster.Entries[boxed.Id].Priority;

    roster.Swap(party, boxed);

    Assert.False(roster.Entries[party.Id].IsInParty);
    Assert.True(roster.Entries[boxed.Id].IsInParty);
    Assert.Equal(partyPriority, roster.Entries[party.Id].Priority);
    Assert.Equal(boxedPriority, roster.Entries[boxed.Id].Priority);
    Assert.DoesNotContain(party.Id, roster.PartyIds);
    Assert.Contains(boxed.Id, roster.PartyIds);
    RosterEntriesSwapped @event = roster.LastChange<RosterEntriesSwapped>();
    Assert.Equal(party.Id, @event.SourceId);
    Assert.False(@event.IsSourceInParty);
    Assert.Equal(boxed.Id, @event.TargetId);
    Assert.True(@event.IsTargetInParty);
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when swapping a Pokémon with itself.")]
  public void Given_SamePokemon_When_Swap_Then_InvalidOperationException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);

    Assert.Throws<InvalidOperationException>(() => roster.Swap(pokemon, pokemon));
  }

  [Fact(DisplayName = "It should throw ArgumentException when a Pokémon is not in the roster.")]
  public void Given_MissingPokemon_When_Swap_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen party = Catalog.CreateOwnedPokemon(Catalog.Red, "party");
    Specimen missing = Catalog.CreateOwnedPokemon(Catalog.Red, "missing");
    roster.Add(party, Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Swap(party, missing));
    Assert.Equal("target", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when a Pokémon belongs to another trainer.")]
  public void Given_OtherTrainer_When_Swap_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen redPokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "red");
    Specimen bluePokemon = Catalog.CreateOwnedPokemon(Catalog.Blue, "blue");
    roster.Add(redPokemon, Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Swap(redPokemon, bluePokemon));
    Assert.Equal("target", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw NotImplementedException when swapping two party Pokémon.")]
  public void Given_PartyAndParty_When_Swap_Then_NotImplementedException()
  {
    Roster roster = new(Catalog.Red);
    Specimen first = Catalog.CreateOwnedPokemon(Catalog.Red, "first");
    Specimen second = Catalog.CreateOwnedPokemon(Catalog.Red, "second");
    roster.Add(first, Catalog.Red);
    roster.Add(second, Catalog.Red);

    Assert.Throws<NotImplementedException>(() => roster.Swap(first, second));
  }

  [Fact(DisplayName = "It should throw NotImplementedException when swapping two boxed Pokémon.")]
  public void Given_BoxAndBox_When_Swap_Then_NotImplementedException()
  {
    (Roster roster, _, Specimen firstBoxed) = CreatePartyAndBoxed();
    Specimen secondBoxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed-2");
    roster.Add(secondBoxed, Catalog.Red);

    Assert.Throws<NotImplementedException>(() => roster.Swap(firstBoxed, secondBoxed));
  }

  private (Roster Roster, Specimen Party, Specimen Boxed) CreatePartyAndBoxed()
  {
    Roster roster = new(Catalog.Red);
    Specimen? party = null;
    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, $"party-{index}");
      roster.Add(pokemon, Catalog.Red);
      party ??= pokemon;
    }
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);
    return (roster, party!, boxed);
  }
}
