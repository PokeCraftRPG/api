using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public readonly struct RosterId : IEntityProvider
{
  private const char Separator = '|';

  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public TrainerId TrainerId { get; }

  public RosterId(StreamId streamId)
  {
    StreamId = streamId;

    string[] values = streamId.Value.Split(Separator);
    if (values.Length != 3)
    {
      throw new ArgumentException($"The value '{streamId}' is not a valid roster identifier.", nameof(streamId));
    }
    TrainerId = new TrainerId(string.Join(Separator, values.Take(2)));
  }

  public RosterId(TrainerId trainerId)
  {
    string value = string.Join(Separator, trainerId, Roster.EntityKind);
    StreamId = new StreamId(value);

    TrainerId = trainerId;
  }

  public RosterId(string value) : this(new StreamId(value))
  {
  }

  public Entity GetEntity() => new(Roster.EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public static bool operator ==(RosterId left, RosterId right) => left.Equals(right);
  public static bool operator !=(RosterId left, RosterId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is RosterId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
