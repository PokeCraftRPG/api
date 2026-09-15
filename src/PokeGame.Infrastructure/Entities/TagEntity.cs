using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;

namespace PokeGame.Infrastructure.Entities;

internal class TagEntity
{
  public int TagId { get; private set; }

  public TrainerEntity? Trainer { get; private set; }
  public int TrainerId { get; private set; }
  public Guid Id { get; private set; }

  public string Name { get; private set; } = string.Empty;
  public byte? Red { get; private set; }
  public byte? Green { get; private set; }
  public byte? Blue { get; private set; }

  public string? CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }

  public string? UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public TagEntity(int trainerId, RosterTagChanged @event)
  {
    TrainerId = trainerId;
    Id = @event.TagId;

    CreatedBy = @event.ActorId?.Value;
    CreatedOn = @event.OccurredOn.AsUniversalTime();

    Update(@event);
  }

  private TagEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(capacity: 2);
    if (CreatedBy is not null)
    {
      actorIds.Add(new ActorId(CreatedBy));
    }
    if (UpdatedBy is not null)
    {
      actorIds.Add(new ActorId(UpdatedBy));
    }
    return actorIds;
  }

  public void Update(RosterTagChanged @event)
  {
    Tag tag = @event.Tag;
    Name = tag.Name.Value;
    Red = tag.Color?.Red;
    Green = tag.Color?.Green;
    Blue = tag.Color?.Blue;

    UpdatedBy = @event.ActorId?.Value;
    UpdatedOn = @event.OccurredOn.AsUniversalTime();
  }

  public override bool Equals(object? obj) => obj is TagEntity tag && tag.TagId == TagId;
  public override int GetHashCode() => TagId.GetHashCode();
  public override string ToString() => $"{Name} | {base.ToString()} (TagId={TagId})";
}
