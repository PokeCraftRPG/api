using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Events;

namespace PokeGame.Infrastructure.Entities;

internal class EvolutionEntity : AggregateEntity
{
  public int EvolutionId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public FormEntity? Source { get; private set; }
  public int SourceId { get; private set; }
  public FormEntity? Target { get; private set; }
  public int TargetId { get; private set; }
  public EvolutionTrigger Trigger { get; private set; }

  public byte? Level { get; private set; }
  public bool Friendship { get; private set; }
  public Gender? Gender { get; private set; }
  public ItemEntity? Item { get; private set; }
  public int? ItemId { get; private set; }
  public MoveEntity? Move { get; private set; }
  public int? MoveId { get; private set; }
  public string? Location { get; private set; }
  public TimeOfDay? TimeOfDay { get; private set; }

  public EvolutionEntity(int worldId, int sourceId, int targetId, int? itemId, EvolutionCreated @event) : base(@event)
  {
    WorldId = worldId;

    Id = new EvolutionId(@event.StreamId).EntityId;

    SourceId = sourceId;
    TargetId = targetId;
    Trigger = @event.Trigger;

    ItemId = itemId;
  }

  private EvolutionEntity()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Source is not null)
    {
      actorIds.AddRange(Source.GetActorIds());
    }
    if (Target is not null)
    {
      actorIds.AddRange(Target.GetActorIds());
    }
    if (Item is not null)
    {
      actorIds.AddRange(Item.GetActorIds());
    }
    if (Move is not null)
    {
      actorIds.AddRange(Move.GetActorIds());
    }
    return actorIds;
  }

  public void SetConditions(int? itemId, int? moveId, EvolutionConditionsChanged @event)
  {
    Update(@event);

    Level = @event.Level?.Value;
    Friendship = @event.Friendship;
    Gender = @event.Gender;
    ItemId = itemId;
    MoveId = moveId;
    Location = @event.Location?.Value;
    TimeOfDay = @event.TimeOfDay;
  }
}
