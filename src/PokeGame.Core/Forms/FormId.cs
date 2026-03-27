using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public readonly struct FormId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public FormId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Form.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public FormId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Form.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public FormId(string value) : this(new StreamId(value))
  {
  }

  public static FormId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public static bool operator ==(FormId left, FormId right) => left.Equals(right);
  public static bool operator !=(FormId left, FormId right) => !left.Equals(right);

  public Entity GetEntity() => new(Form.EntityKind, EntityId, WorldId);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is FormId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
