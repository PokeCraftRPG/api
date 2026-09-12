using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public readonly struct RosterId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public TrainerId TrainerId { get; }

  public RosterId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Roster.EntityKind);
    if (!entity.WorldId.HasValue)
    {
      throw new ArgumentException("A world identifier is required.", nameof(streamId));
    }
    TrainerId = new TrainerId(entity.WorldId.Value, entity.Id);
  }

  public RosterId(string value) : this(new StreamId(value))
  {
  }

  public RosterId(TrainerId trainerId)
  {
    Entity entity = new(Roster.EntityKind, trainerId.EntityId, trainerId.WorldId);
    StreamId = new StreamId(entity.ToString());

    TrainerId = trainerId;
  }

  public Entity GetEntity() => new(Roster.EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public static bool operator ==(RosterId left, RosterId right) => left.Equals(right);
  public static bool operator !=(RosterId left, RosterId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is RosterId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
