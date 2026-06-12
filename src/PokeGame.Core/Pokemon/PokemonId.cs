using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

public readonly struct PokemonId
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public PokemonId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Specimen.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("A world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public PokemonId(string value) : this(new StreamId(value))
  {
  }

  public PokemonId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Specimen.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public static PokemonId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public static bool operator ==(PokemonId left, PokemonId right) => left.Equals(right);
  public static bool operator !=(PokemonId left, PokemonId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is PokemonId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
