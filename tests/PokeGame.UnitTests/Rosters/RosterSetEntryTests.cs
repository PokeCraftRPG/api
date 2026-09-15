using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Rosters;

public class RosterSetEntryTests : UnitTests
{
  [Fact(DisplayName = "It should set the priority of a roster entry.")]
  public void Given_Entry_When_SetPriority_Then_RosterEntryChanged()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "priority");
    roster.Add(pokemon, Catalog.Red);
    roster.ClearChanges();

    roster.SetEntry(pokemon, priority: 42, tagIds: []);

    Assert.Equal(42, roster.Entries[pokemon.Id].Priority);
    Assert.True(roster.Entries[pokemon.Id].IsInParty);
    RosterEntryChanged @event = roster.LastChange<RosterEntryChanged>();
    Assert.Equal(pokemon.Id, @event.PokemonId);
    Assert.Equal(42, @event.Priority);
    Assert.Empty(@event.TagIds);
  }

  [Fact(DisplayName = "It should set tags on a roster entry.")]
  public void Given_Tags_When_SetEntry_Then_TagsAssigned()
  {
    Roster roster = new(Catalog.Red);
    Guid favoriteId = Guid.NewGuid();
    Guid competitiveId = Guid.NewGuid();
    roster.SetTag(favoriteId, new Tag(new Name("Favorites"), new Color(255, 0, 0)));
    roster.SetTag(competitiveId, new Tag(new Name("Competitive"), null));
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "tagged");
    roster.Add(pokemon, Catalog.Red);
    roster.ClearChanges();

    roster.SetEntry(pokemon, priority: 10, tagIds: [competitiveId, favoriteId, favoriteId]);

    Assert.Equal(10, roster.Entries[pokemon.Id].Priority);
    Assert.Equal(2, roster.Entries[pokemon.Id].TagIds.Count);
    Assert.Contains(favoriteId, roster.Entries[pokemon.Id].TagIds);
    Assert.Contains(competitiveId, roster.Entries[pokemon.Id].TagIds);
    RosterEntryChanged @event = roster.LastChange<RosterEntryChanged>();
    Assert.Equal(2, @event.TagIds.Count);
    Assert.Contains(favoriteId, @event.TagIds);
    Assert.Contains(competitiveId, @event.TagIds);
  }

  [Fact(DisplayName = "It should clear tags when an empty list is provided.")]
  public void Given_TaggedEntry_When_SetEmptyTags_Then_TagsCleared()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    roster.SetTag(tagId, new Tag(new Name("Favorites"), null));
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "cleared");
    roster.Add(pokemon, Catalog.Red);
    roster.SetEntry(pokemon, priority: 5, tagIds: [tagId]);
    roster.ClearChanges();

    roster.SetEntry(pokemon, priority: 5, tagIds: []);

    Assert.Empty(roster.Entries[pokemon.Id].TagIds);
    Assert.Empty(roster.LastChange<RosterEntryChanged>().TagIds);
  }

  [Fact(DisplayName = "It should preserve party status when changing priority and tags.")]
  public void Given_BoxedPokemon_When_SetEntry_Then_RemainsBoxed()
  {
    Roster roster = FillParty(Catalog.Red);
    Specimen boxed = Catalog.CreateOwnedPokemon(Catalog.Red, "boxed");
    roster.Add(boxed, Catalog.Red);
    roster.ClearChanges();

    roster.SetEntry(boxed, priority: 7, tagIds: []);

    Assert.False(roster.Entries[boxed.Id].IsInParty);
    Assert.Equal(7, roster.Entries[boxed.Id].Priority);
  }

  [Fact(DisplayName = "It should not raise an event when the entry is unchanged.")]
  public void Given_SameValues_When_SetEntry_Then_NoEvent()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    roster.SetTag(tagId, new Tag(new Name("Stable"), null));
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "stable");
    roster.Add(pokemon, Catalog.Red);
    roster.SetEntry(pokemon, priority: 3, tagIds: [tagId]);
    roster.ClearChanges();

    roster.SetEntry(pokemon, priority: 3, tagIds: [tagId]);

    Assert.False(roster.HasChanges);
  }

  [Fact(DisplayName = "It should restore priority and tags after replaying events.")]
  public void Given_Changes_When_Replay_Then_StateRestored()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    roster.SetTag(tagId, new Tag(new Name("Replay"), null));
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "replay");
    roster.Add(pokemon, Catalog.Red);
    roster.SetEntry(pokemon, priority: 55, tagIds: [tagId]);

    Roster replayed = roster.Replay();

    Assert.Equal(55, replayed.Entries[pokemon.Id].Priority);
    Assert.Contains(tagId, replayed.Entries[pokemon.Id].TagIds);
    Assert.True(replayed.Entries[pokemon.Id].IsInParty);
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when the priority is below the minimum.")]
  public void Given_PriorityBelowMinimum_When_SetEntry_Then_ArgumentOutOfRangeException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);

    Assert.Throws<ArgumentOutOfRangeException>(() => roster.SetEntry(pokemon, Roster.MinimumPriority - 1, []));
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when the priority is above the maximum.")]
  public void Given_PriorityAboveMaximum_When_SetEntry_Then_ArgumentOutOfRangeException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);

    Assert.Throws<ArgumentOutOfRangeException>(() => roster.SetEntry(pokemon, Roster.MaximumPriority + 1, []));
  }

  [Fact(DisplayName = "It should throw PokemonNotInRosterException when the Pokémon is not in the roster.")]
  public void Given_NotInRoster_When_SetEntry_Then_PokemonNotInRosterException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);

    PokemonNotInRosterException exception = Assert.Throws<PokemonNotInRosterException>(
      () => roster.SetEntry(pokemon, priority: 1, tagIds: []));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw TagsNotFoundException when a tag is unknown.")]
  public void Given_UnknownTag_When_SetEntry_Then_TagsNotFoundException()
  {
    Roster roster = new(Catalog.Red);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    roster.Add(pokemon, Catalog.Red);
    Guid unknownTagId = Guid.NewGuid();

    TagsNotFoundException exception = Assert.Throws<TagsNotFoundException>(
      () => roster.SetEntry(pokemon, priority: 1, tagIds: [unknownTagId]));
    Assert.Equal(Catalog.Red.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Catalog.Red.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(nameof(RosterEntry.TagIds), exception.Data["PropertyName"]);
    Assert.Equal([unknownTagId], Assert.IsAssignableFrom<IReadOnlyCollection<Guid>>(exception.Data["TagIds"]));
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the Pokémon belongs to another world.")]
  public void Given_DifferentWorld_When_SetEntry_Then_WorldMismatchException()
  {
    Roster roster = new(Catalog.Red);
    DomainCatalog other = new(Faker);

    Assert.Throws<WorldMismatchException>(() => roster.SetEntry(other.CreateOwnedPokemon(other.Red), priority: 1, tagIds: []));
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
}
