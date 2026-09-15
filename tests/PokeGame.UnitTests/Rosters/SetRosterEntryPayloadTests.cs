using PokeGame.Core;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Rosters;

public class SetRosterEntryPayloadTests : UnitTests
{
  [Fact(DisplayName = "It should accept a valid payload.")]
  public void Given_ValidPayload_When_Validate_Then_Valid()
  {
    SetRosterEntryPayload payload = new()
    {
      Priority = Roster.MaximumPriority,
      TagIds = [Guid.NewGuid(), Guid.NewGuid()]
    };

    payload.Validate();
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the priority is below the minimum.")]
  public void Given_PriorityBelowMinimum_When_Validate_Then_InvalidCommandException()
  {
    SetRosterEntryPayload payload = new()
    {
      Priority = Roster.MinimumPriority - 1
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the priority is above the maximum.")]
  public void Given_PriorityAboveMaximum_When_Validate_Then_InvalidCommandException()
  {
    SetRosterEntryPayload payload = new()
    {
      Priority = Roster.MaximumPriority + 1
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when tag ids contain duplicates.")]
  public void Given_DuplicateTagIds_When_Validate_Then_InvalidCommandException()
  {
    Guid tagId = Guid.NewGuid();
    SetRosterEntryPayload payload = new()
    {
      TagIds = [tagId, tagId]
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when there are too many tag ids.")]
  public void Given_TooManyTagIds_When_Validate_Then_InvalidCommandException()
  {
    SetRosterEntryPayload payload = new()
    {
      TagIds = [.. Enumerable.Range(0, 11).Select(_ => Guid.NewGuid())]
    };

    Assert.Throws<InvalidCommandException>(payload.Validate);
  }
}
