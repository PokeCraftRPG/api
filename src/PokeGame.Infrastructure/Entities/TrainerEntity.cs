using Logitar.EventSourcing;
using PokeGame.Core.Trainers;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Infrastructure.Entities;

internal class TrainerEntity : AggregateEntity
{
  public int TrainerId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  // TODO(fpion): License
  // TODO(fpion): Gender
  // TODO(fpion): Money
  // TODO(fpion): Sprite
  // TODO(fpion): User/Member

  public TrainerEntity(int worldId, TrainerCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = new TrainerId(@event.StreamId).EntityId;

    Key = @event.Key.Value;
  }

  private TrainerEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    return actorIds;
  }

  public void SetDetails(TrainerDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(TrainerKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
