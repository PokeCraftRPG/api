using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Trainers;

public class TrainerPartyLimitTests : UnitTests
{
  [Fact(DisplayName = "It should set the party limit.")]
  public void Given_ValidLimit_When_SetPartyLimit_Then_Changed()
  {
    Trainer trainer = Catalog.Red;
    trainer.ClearChanges();

    trainer.SetPartyLimit(3);

    Assert.Equal(3, trainer.PartyLimit);
    TrainerPartyLimitChanged @event = trainer.LastChange<TrainerPartyLimitChanged>();
    Assert.Equal(3, @event.PartyLimit);
  }

  [Fact(DisplayName = "It should clear the party limit.")]
  public void Given_ExistingLimit_When_SetNull_Then_Cleared()
  {
    Trainer trainer = Catalog.Red;
    trainer.SetPartyLimit(2);
    trainer.ClearChanges();

    trainer.SetPartyLimit(null);

    Assert.Null(trainer.PartyLimit);
    TrainerPartyLimitChanged @event = trainer.LastChange<TrainerPartyLimitChanged>();
    Assert.Null(@event.PartyLimit);
  }

  [Fact(DisplayName = "It should not raise an event when the party limit does not change.")]
  public void Given_SameLimit_When_SetPartyLimit_Then_NoEvent()
  {
    Trainer trainer = Catalog.Red;
    trainer.SetPartyLimit(4);
    trainer.ClearChanges();

    trainer.SetPartyLimit(4);

    Assert.False(trainer.HasChanges);
    Assert.Equal(4, trainer.PartyLimit);
  }

  [Theory(DisplayName = "It should throw ArgumentOutOfRangeException when the party limit is invalid.")]
  [InlineData(-1)]
  [InlineData(Roster.PartyLimit + 1)]
  public void Given_InvalidLimit_When_SetPartyLimit_Then_ArgumentOutOfRangeException(int partyLimit)
  {
    Trainer trainer = Catalog.Red;

    ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => trainer.SetPartyLimit(partyLimit));
    Assert.Equal("partyLimit", exception.ParamName);
  }
}
