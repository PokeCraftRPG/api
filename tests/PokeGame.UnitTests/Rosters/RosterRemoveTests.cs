using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Rosters;

public class RosterRemoveTests : UnitTests
{
  [Fact(DisplayName = "It should remove a party Pokémon from the roster.")]
  public void Given_PartyPokemonReleased_When_Remove_Then_RemovedFromParty()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "party");
    roster.Add(pokemon, Catalog.Red);
    pokemon.Release();

    roster.Remove(pokemon);

    Assert.Empty(roster.Entries);
    Assert.Empty(roster.PartyIds);
    RosterEntryRemoved @event = roster.LastChange<RosterEntryRemoved>();
    Assert.Equal(pokemon.Id, @event.PokemonId);
  }

  [Fact(DisplayName = "It should remove a boxed Pokémon from the roster.")]
  public void Given_BoxedPokemonReleased_When_Remove_Then_Removed()
  {
    Roster roster = new(Catalog.Red);
    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      roster.Add(Catalog.CreateOwnedPokemon(Catalog.Red, $"party-{index}"), Catalog.Red);
    }
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);
    boxed.Release();

    roster.Remove(boxed);

    Assert.False(roster.Entries.ContainsKey(boxed.Id));
    Assert.Equal(Roster.PartyLimit, roster.PartyIds.Count);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is still owned by the trainer.")]
  public void Given_StillOwned_When_Remove_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Remove(pokemon));
    Assert.Equal("specimen", exception.ParamName);
  }

  [Fact(DisplayName = "It should throw ArgumentException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_Remove_Then_ArgumentException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Blue);
    pokemon.Release();

    ArgumentException exception = Assert.Throws<ArgumentException>(() => roster.Remove(pokemon));
    Assert.Equal("specimen", exception.ParamName);
  }
}
