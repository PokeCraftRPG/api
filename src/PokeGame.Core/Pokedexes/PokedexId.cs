using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Pokedexes;

public readonly struct PokedexId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public TrainerId TrainerId { get; }

  public PokedexId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Pokedex.EntityKind);
    if (!entity.WorldId.HasValue)
    {
      throw new ArgumentException("A world identifier is required.", nameof(streamId));
    }
    TrainerId = new TrainerId(entity.WorldId.Value, entity.Id);
  }

  public PokedexId(string value) : this(new StreamId(value))
  {
  }

  public PokedexId(TrainerId trainerId)
  {
    Entity entity = new(Pokedex.EntityKind, trainerId.EntityId, trainerId.WorldId);
    StreamId = new StreamId(entity.ToString());

    TrainerId = trainerId;
  }

  public Entity GetEntity() => new(Pokedex.EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public static bool operator ==(PokedexId left, PokedexId right) => left.Equals(right);
  public static bool operator !=(PokedexId left, PokedexId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is PokedexId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
