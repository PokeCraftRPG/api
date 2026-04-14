using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public class Roster : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Roster";

  public new RosterId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  public Roster() : base()
  {
  }

  public Roster(Trainer trainer) : base(new RosterId(trainer.Id).StreamId)
  {
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);
}
