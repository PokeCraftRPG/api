using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Rosters;

public class RosterReplaceTests : UnitTests
{
  [Fact(DisplayName = "It should replace a party Pokémon and keep the incoming Pokémon in the party.")]
  public void Given_PartySlot_When_Replace_Then_IncomingInParty()
  {
    (Roster roster, Specimen source, Specimen target) = CreateTradedPair(fillParty: false);
    roster.Replace(source, target);

    Assert.False(roster.Entries.ContainsKey(source.Id));
    Assert.True(roster.Entries[target.Id].IsInParty);
    Assert.Equal(0, roster.Entries[target.Id].Priority);
    Assert.Contains(target.Id, roster.PartyIds);
    Assert.DoesNotContain(source.Id, roster.PartyIds);
    RosterEntryReplaced @event = roster.LastChange<RosterEntryReplaced>();
    Assert.Equal(source.Id, @event.SourceId);
    Assert.Equal(target.Id, @event.TargetId);
    Assert.True(@event.IsInParty);
  }

  [Fact(DisplayName = "It should replace a boxed Pokémon and keep the incoming Pokémon in the box.")]
  public void Given_BoxedSlot_When_Replace_Then_IncomingInBox()
  {
    (Roster roster, Specimen source, Specimen target) = CreateTradedPair(fillParty: true);

    roster.Replace(source, target);

    Assert.False(roster.Entries[target.Id].IsInParty);
    Assert.DoesNotContain(target.Id, roster.PartyIds);
    Assert.False(roster.LastChange<RosterEntryReplaced>().IsInParty);
  }

  [Fact(DisplayName = "It should send an incoming egg to the box even when replacing a party Pokémon.")]
  public void Given_PartySlotAndEgg_When_Replace_Then_IncomingInBox()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "egg", eggCycles: 5);
    Roster roster = new(Catalog.Red);
    roster.Add(source, Catalog.Red);
    source.Trade(target, Catalog.PokemonCenter);

    roster.Replace(source, target);

    Assert.False(roster.Entries[target.Id].IsInParty);
    Assert.False(roster.LastChange<RosterEntryReplaced>().IsInParty);
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when replacing a Pokémon with itself.")]
  public void Given_SamePokemon_When_Replace_Then_InvalidOperationException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);
    pokemon.Release();

    Assert.Throws<InvalidOperationException>(() => roster.Replace(pokemon, pokemon));
  }

  [Fact(DisplayName = "It should throw UnexpectedPokemonOwnerException when the source is still owned by the trainer.")]
  public void Given_SourceStillOwned_When_Replace_Then_UnexpectedPokemonOwnerException()
  {
    Roster roster = new(Catalog.Red);
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Red, "target");
    roster.Add(source, Catalog.Red);

    UnexpectedPokemonOwnerException exception = Assert.Throws<UnexpectedPokemonOwnerException>(
      () => roster.Replace(source, target));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(source.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonOwnerException when the target is not owned by the trainer.")]
  public void Given_TargetNotOwned_When_Replace_Then_InvalidPokemonOwnerException()
  {
    Roster roster = new(Catalog.Red);
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreatePokemon("wild");
    roster.Add(source, Catalog.Red);
    source.Release();

    InvalidPokemonOwnerException exception = Assert.Throws<InvalidPokemonOwnerException>(
      () => roster.Replace(source, target));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(target.EntityId, exception.Data["PokemonId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["ExpectedTrainerId"]);
    Assert.Null(exception.Data["AttemptedTrainerId"]);
  }

  [Fact(DisplayName = "It should throw PokemonNotInRosterException when the source is not in the roster.")]
  public void Given_SourceMissing_When_Replace_Then_PokemonNotInRosterException()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "target");
    source.Trade(target, Catalog.PokemonCenter);
    Roster roster = new(Catalog.Red);

    PokemonNotInRosterException exception = Assert.Throws<PokemonNotInRosterException>(
      () => roster.Replace(source, target));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(source.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw UnexpectedPokemonRosterException when the target is already in the roster.")]
  public void Given_TargetAlreadyInRoster_When_Replace_Then_UnexpectedPokemonRosterException()
  {
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Red, "target");
    Roster roster = new(Catalog.Red);
    roster.Add(source, Catalog.Red);
    roster.Add(target, Catalog.Red);
    source.Release();

    UnexpectedPokemonRosterException exception = Assert.Throws<UnexpectedPokemonRosterException>(
      () => roster.Replace(source, target));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(target.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when a Pokémon belongs to another world.")]
  public void Given_DifferentWorld_When_Replace_Then_WorldMismatchException()
  {
    Roster roster = new(Catalog.Red);
    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    roster.Add(source, Catalog.Red);
    source.Release();
    DomainCatalog other = new(Faker);

    Assert.Throws<WorldMismatchException>(() => roster.Replace(source, other.CreateOwnedPokemon(other.Red)));
  }

  private (Roster Roster, Specimen Source, Specimen Target) CreateTradedPair(bool fillParty)
  {
    Roster roster = new(Catalog.Red);
    if (fillParty)
    {
      for (int index = 0; index < Roster.PartyLimit; index++)
      {
        roster.Add(Catalog.CreateOwnedPokemon(Catalog.Red, $"party-{index}"), Catalog.Red);
      }
    }

    Specimen source = Catalog.CreateOwnedPokemon(Catalog.Red, "source");
    Specimen target = Catalog.CreateOwnedPokemon(Catalog.Blue, "target", pokeBall: Catalog.PokeBall);
    roster.Add(source, Catalog.Red);
    source.Trade(target, Catalog.PokemonCenter);
    return (roster, source, target);
  }
}
