using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public readonly struct SpecimenId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public SpecimenId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Specimen.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public SpecimenId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Specimen.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public SpecimenId(string value) : this(new StreamId(value))
  {
  }

  public static SpecimenId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Specimen.EntityKind, EntityId, WorldId);

  public static bool operator ==(SpecimenId left, SpecimenId right) => left.Equals(right);
  public static bool operator !=(SpecimenId left, SpecimenId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is SpecimenId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
