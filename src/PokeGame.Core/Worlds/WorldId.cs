using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds;

public readonly struct WorldId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId(StreamId streamId)
  {
    StreamId = streamId;
  }

  public WorldId(Guid id)
  {
    Entity entity = new(World.EntityKind, id);
    StreamId = new StreamId(entity.ToString());
  }

  public WorldId(string value) : this(new StreamId(value))
  {
  }

  public static WorldId NewId() => new(Guid.NewGuid());
  public Guid ToGuid() => Entity.Parse(Value, World.EntityKind).Id;

  public Entity GetEntity() => new(World.EntityKind, ToGuid());

  public static bool operator ==(WorldId left, WorldId right) => left.Equals(right);
  public static bool operator !=(WorldId left, WorldId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is WorldId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
