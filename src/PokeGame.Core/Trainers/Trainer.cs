using Logitar.EventSourcing;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public sealed class Trainer : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Trainer";

  public new TrainerId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  // TODO(fpion): License
  // TODO(fpion): Gender
  // TODO(fpion): Money
  // TODO(fpion): Sprite
  // TODO(fpion): User/Member

  public Trainer() : base()
  {
  }

  public Trainer(World world, Key key, ActorId? actorId = null)
    : this(TrainerId.NewId(world.Id), key, actorId)
  {
  }

  public Trainer(TrainerId trainerId, Key key, ActorId? actorId = null)
    : base(trainerId.StreamId)
  {
    Raise(new TrainerCreated(key), actorId);
  }
  private void Handle(TrainerCreated @event)
  {
    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new TrainerDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetDetails(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new TrainerDetailsChanged(name, summary, content), actorId);
    }
  }
  private void Handle(TrainerDetailsChanged @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new TrainerKeyChanged(key), actorId);
    }
  }
  private void Handle(TrainerKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
