using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public class Trainer : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Trainer";

  public new TrainerId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public Trainer() : base()
  {
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);
}
