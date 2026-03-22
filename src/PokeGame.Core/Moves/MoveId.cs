using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public readonly struct MoveId
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public MoveId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Move.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public MoveId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Move.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public MoveId(string value) : this(new StreamId(value))
  {
  }

  public static MoveId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public static bool operator ==(MoveId left, MoveId right) => left.Equals(right);
  public static bool operator !=(MoveId left, MoveId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is MoveId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
