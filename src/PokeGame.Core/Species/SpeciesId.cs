using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public readonly struct SpeciesId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public SpeciesId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, PokemonSpecies.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public SpeciesId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(PokemonSpecies.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public SpeciesId(string value) : this(new StreamId(value))
  {
  }

  public static SpeciesId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(PokemonSpecies.EntityKind, EntityId, WorldId);

  public static bool operator ==(SpeciesId left, SpeciesId right) => left.Equals(right);
  public static bool operator !=(SpeciesId left, SpeciesId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is SpeciesId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
