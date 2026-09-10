using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Moves;
using PokeGame.Core.Varieties.Events;

namespace PokeGame.Infrastructure.Entities;

internal class VarietyMoveEntity
{
  public int VarietyMoveId { get; private set; }

  public VarietyEntity? Variety { get; private set; }
  public int VarietyId { get; private set; }
  public Guid Id { get; private set; }

  public MoveEntity? Move { get; private set; }
  public int MoveId { get; private set; }

  public LearningMethod LearningMethod { get; private set; }
  public int? Level { get; private set; }

  public string? CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }

  public string? UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public VarietyMoveEntity(VarietyEntity variety, int moveId, VarietyMoveChanged @event)
  {
    Variety = variety;
    VarietyId = variety.VarietyId;
    Id = @event.VarietyMoveId;

    MoveId = moveId;

    CreatedBy = @event.ActorId?.Value;
    CreatedOn = @event.OccurredOn.AsUniversalTime();

    Update(@event);
  }

  private VarietyMoveEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new();
    if (Move is not null)
    {
      actorIds.AddRange(Move.GetActorIds());
    }
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

  public void Update(VarietyMoveChanged @event)
  {
    LearningMethod = @event.Move.LearningMethod;
    Level = @event.Move.Level?.Value;

    UpdatedBy = @event.ActorId?.Value;
    UpdatedOn = @event.OccurredOn.AsUniversalTime();
  }

  public override bool Equals(object? obj) => obj is VarietyMoveEntity entity && entity.VarietyMoveId == VarietyMoveId;
  public override int GetHashCode() => VarietyMoveId.GetHashCode();
  public override string ToString() => $"{base.ToString()} (VarietyMoveId={VarietyMoveId})";
}
