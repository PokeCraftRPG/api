using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Abilities;

public readonly struct AbilityId
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public AbilityId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Ability.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public AbilityId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Ability.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public AbilityId(string value) : this(new StreamId(value))
  {
  }

  public static AbilityId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public static bool operator ==(AbilityId left, AbilityId right) => left.Equals(right);
  public static bool operator !=(AbilityId left, AbilityId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is AbilityId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
