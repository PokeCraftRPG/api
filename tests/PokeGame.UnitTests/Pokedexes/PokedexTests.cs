using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Pokedexes;
using PokeGame.Core.Pokedexes.Events;

namespace PokeGame.Pokedexes;

public class PokedexTests : UnitTests
{
  [Fact(DisplayName = "It should register a variety as acquired.")]
  public void Given_MissingEntry_When_RegisterAcquired_Then_PokedexEntryAcquired()
  {
    Pokedex pokedex = new(Catalog.Red);

    pokedex.RegisterAcquired(Catalog.Variety.Id);

    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[Catalog.Variety.Id]);
    PokedexEntryAcquired @event = pokedex.LastChange<PokedexEntryAcquired>();
    Assert.Equal(Catalog.Variety.Id, @event.VarietyId);
  }

  [Fact(DisplayName = "It should upgrade a seen entry to acquired.")]
  public void Given_SeenEntry_When_RegisterAcquired_Then_UpgradedToAcquired()
  {
    Pokedex pokedex = new(Catalog.Red);
    pokedex.RegisterSeen(Catalog.Variety.Id);
    pokedex.ClearChanges();

    pokedex.RegisterAcquired(Catalog.Variety.Id);

    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[Catalog.Variety.Id]);
    Assert.Equal(Catalog.Variety.Id, pokedex.LastChange<PokedexEntryAcquired>().VarietyId);
  }

  [Fact(DisplayName = "It should not raise an event when the variety is already acquired.")]
  public void Given_AcquiredEntry_When_RegisterAcquired_Then_NoEvent()
  {
    Pokedex pokedex = new(Catalog.Red);
    pokedex.RegisterAcquired(Catalog.Variety.Id);
    pokedex.ClearChanges();

    pokedex.RegisterAcquired(Catalog.Variety.Id);

    Assert.False(pokedex.HasChanges);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[Catalog.Variety.Id]);
  }

  [Fact(DisplayName = "It should register a variety as seen.")]
  public void Given_MissingEntry_When_RegisterSeen_Then_PokedexEntrySeen()
  {
    Pokedex pokedex = new(Catalog.Red);

    pokedex.RegisterSeen(Catalog.Variety.Id);

    Assert.Equal(PokedexEntryStatus.Seen, pokedex.Entries[Catalog.Variety.Id]);
    PokedexEntrySeen @event = pokedex.LastChange<PokedexEntrySeen>();
    Assert.Equal(Catalog.Variety.Id, @event.VarietyId);
  }

  [Fact(DisplayName = "It should not raise an event when registering seen for an existing entry.")]
  public void Given_ExistingEntry_When_RegisterSeen_Then_NoEvent()
  {
    Pokedex pokedex = new(Catalog.Red);
    pokedex.RegisterSeen(Catalog.Variety.Id);
    pokedex.ClearChanges();

    pokedex.RegisterSeen(Catalog.Variety.Id);

    Assert.False(pokedex.HasChanges);
    Assert.Equal(PokedexEntryStatus.Seen, pokedex.Entries[Catalog.Variety.Id]);
  }

  [Fact(DisplayName = "It should not downgrade an acquired entry to seen.")]
  public void Given_AcquiredEntry_When_RegisterSeen_Then_NoEvent()
  {
    Pokedex pokedex = new(Catalog.Red);
    pokedex.RegisterAcquired(Catalog.Variety.Id);
    pokedex.ClearChanges();

    pokedex.RegisterSeen(Catalog.Variety.Id);

    Assert.False(pokedex.HasChanges);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[Catalog.Variety.Id]);
  }

  [Fact(DisplayName = "It should track multiple varieties independently.")]
  public void Given_MultipleVarieties_When_Register_Then_EntriesTracked()
  {
    Pokedex pokedex = new(Catalog.Red);

    pokedex.RegisterSeen(Catalog.Variety.Id);
    pokedex.RegisterAcquired(Catalog.CharmanderVariety.Id);

    Assert.Equal(PokedexEntryStatus.Seen, pokedex.Entries[Catalog.Variety.Id]);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[Catalog.CharmanderVariety.Id]);
  }

  [Fact(DisplayName = "It should restore entries after replaying uncommitted events.")]
  public void Given_Changes_When_Replay_Then_StateRestored()
  {
    Pokedex pokedex = new(Catalog.Red);
    pokedex.RegisterSeen(Catalog.Variety.Id);
    pokedex.RegisterAcquired(Catalog.Variety.Id);
    pokedex.RegisterAcquired(Catalog.CharmanderVariety.Id);

    Pokedex replayed = pokedex.Replay();

    Assert.Equal(PokedexEntryStatus.Acquired, replayed.Entries[Catalog.Variety.Id]);
    Assert.Equal(PokedexEntryStatus.Acquired, replayed.Entries[Catalog.CharmanderVariety.Id]);
  }

  [Fact(DisplayName = "It should expose the trainer identity on the pokedex id.")]
  public void Given_Trainer_When_Create_Then_IdMatchesTrainer()
  {
    Pokedex pokedex = new(Catalog.Red);

    Assert.Equal(Catalog.Red.Id, pokedex.TrainerId);
    Assert.Equal(Catalog.Red.Id, pokedex.Id.TrainerId);
    Assert.Equal(Pokedex.EntityKind, pokedex.GetEntity().Kind);
  }

  [Fact(DisplayName = "It should round-trip a pokedex id from its stream value.")]
  public void Given_PokedexId_When_Parse_Then_Equal()
  {
    PokedexId id = new(Catalog.Red.Id);

    PokedexId parsed = new(id.Value);

    Assert.Equal(id, parsed);
    Assert.Equal(Catalog.Red.Id, parsed.TrainerId);
  }

  [Fact(DisplayName = "It should throw when the stream id has no world.")]
  public void Given_StreamWithoutWorld_When_CreatePokedexId_Then_ArgumentException()
  {
    StreamId streamId = new(new Entity(Pokedex.EntityKind, Guid.NewGuid()).ToString());

    Assert.Throws<ArgumentException>(() => new PokedexId(streamId));
  }
}
