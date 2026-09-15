using PokeGame.Core;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Rosters;

public class RosterTagTests : UnitTests
{
  [Fact(DisplayName = "It should add a tag with a generated identifier.")]
  public void Given_Tag_When_AddTag_Then_Added()
  {
    Roster roster = new(Catalog.Red);
    Tag tag = new(new Name("Favorites"), new Color(255, 0, 0));

    roster.AddTag(tag);

    Guid tagId = Assert.Single(roster.Tags).Key;
    Assert.Equal(tag, roster.Tags[tagId]);
    RosterTagChanged @event = roster.LastChange<RosterTagChanged>();
    Assert.Equal(tagId, @event.TagId);
    Assert.Equal(tag, @event.Tag);
  }

  [Fact(DisplayName = "It should set a tag with the provided identifier.")]
  public void Given_TagId_When_SetTag_Then_Set()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    Tag tag = new(new Name("Competitive"), null);

    roster.SetTag(tagId, tag);

    Assert.Equal(tag, roster.FindTag(tagId));
    Assert.True(roster.HasTag(tagId));
    Assert.Equal(tagId, roster.LastChange<RosterTagChanged>().TagId);
  }

  [Fact(DisplayName = "It should replace an existing tag when its value changes.")]
  public void Given_ExistingTag_When_SetTagChanged_Then_Replaced()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    roster.SetTag(tagId, new Tag(new Name("Old"), null));
    roster.ClearChanges();

    Tag updated = new(new Name("New"), new Color(0, 128, 255));
    roster.SetTag(tagId, updated);

    Assert.Equal(updated, roster.FindTag(tagId));
    Assert.Equal(updated, roster.LastChange<RosterTagChanged>().Tag);
  }

  [Fact(DisplayName = "It should not raise an event when the tag is unchanged.")]
  public void Given_SameTag_When_SetTag_Then_Unchanged()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    Tag tag = new(new Name("Stable"), new Color(1, 2, 3));
    roster.SetTag(tagId, tag);
    roster.ClearChanges();

    roster.SetTag(tagId, tag);

    Assert.Empty(roster.Changes);
  }

  [Fact(DisplayName = "It should remove an existing tag.")]
  public void Given_ExistingTag_When_RemoveTag_Then_Removed()
  {
    Roster roster = new(Catalog.Red);
    Guid tagId = Guid.NewGuid();
    roster.SetTag(tagId, new Tag(new Name("Temporary"), null));
    roster.ClearChanges();

    roster.RemoveTag(tagId);

    Assert.False(roster.HasTag(tagId));
    Assert.Null(roster.TryGetTag(tagId));
    Assert.Equal(tagId, roster.LastChange<RosterTagRemoved>().TagId);
  }

  [Fact(DisplayName = "It should remove a deleted tag from roster entries.")]
  public void Given_TaggedEntries_When_RemoveTag_Then_RemovedFromEntries()
  {
    Roster roster = new(Catalog.Red);
    Guid keepId = Guid.NewGuid();
    Guid removeId = Guid.NewGuid();
    roster.SetTag(keepId, new Tag(new Name("Keep"), null));
    roster.SetTag(removeId, new Tag(new Name("Remove"), null));
    Specimen first = Catalog.CreateOwnedPokemon(Catalog.Red, "first");
    Specimen second = Catalog.CreateOwnedPokemon(Catalog.Red, "second");
    roster.Add(first, Catalog.Red);
    roster.Add(second, Catalog.Red);
    roster.SetEntry(first, priority: 1, tagIds: [keepId, removeId]);
    roster.SetEntry(second, priority: 2, tagIds: [removeId]);
    roster.ClearChanges();

    roster.RemoveTag(removeId);

    Assert.False(roster.HasTag(removeId));
    Assert.Equal([keepId], roster.Entries[first.Id].TagIds);
    Assert.Empty(roster.Entries[second.Id].TagIds);
    Assert.Equal(1, roster.Entries[first.Id].Priority);
    Assert.Equal(2, roster.Entries[second.Id].Priority);
  }

  [Fact(DisplayName = "It should not raise an event when removing a missing tag.")]
  public void Given_MissingTag_When_RemoveTag_Then_Unchanged()
  {
    Roster roster = new(Catalog.Red);

    roster.RemoveTag(Guid.NewGuid());

    Assert.Empty(roster.Changes);
  }

  [Fact(DisplayName = "It should throw when finding a missing tag.")]
  public void Given_MissingTag_When_FindTag_Then_InvalidOperationException()
  {
    Roster roster = new(Catalog.Red);

    Assert.Throws<InvalidOperationException>(() => roster.FindTag(Guid.NewGuid()));
  }
}
