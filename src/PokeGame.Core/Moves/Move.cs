using Logitar.EventSourcing;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public sealed class Move : AggregateRoot, IEntityProvider
{
  // TODO(fpion): Power should be null when Category == Status.

  public const string EntityKind = "Move";

  public new MoveId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  private Key? _key = null;
  public Key Key => _key ?? throw new InvalidOperationException("The key was not initialized.");

  public Name? Name { get; private set; }
  public Summary? Summary { get; private set; }
  public Content? Content { get; private set; }

  public Accuracy? Accuracy { get; private set; }
  public Power? Power { get; private set; }
  public PowerPoints? PowerPoints { get; private set; }

  public Move() : base()
  {
  }

  public Move(World world, PokemonType type, MoveCategory category, Key key, ActorId? actorId = null)
    : this(MoveId.NewId(world.Id), type, category, key, actorId)
  {
  }

  public Move(MoveId moveId, PokemonType type, MoveCategory category, Key key, ActorId? actorId = null)
    : base(moveId.StreamId)
  {
    if (!Enum.IsDefined(type))
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new MoveCreated(type, category, key), actorId);
  }
  private void Handle(MoveCreated @event)
  {
    Type = @event.Type;
    Category = @event.Category;

    _key = @event.Key;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new MoveDeleted(), actorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void SetKey(Key key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new MoveKeyChanged(key), actorId);
    }
  }
  private void Handle(MoveKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetMechanics(Accuracy? accuracy, Power? power, PowerPoints? powerPoints, ActorId? actorId = null)
  {
    if (!Equals(Accuracy, accuracy) || !Equals(Power, power) || !Equals(PowerPoints, powerPoints))
    {
      Raise(new MoveMechanicsChanged(accuracy, power, powerPoints), actorId);
    }
  }
  private void Handle(MoveMechanicsChanged @event)
  {
    Accuracy = @event.Accuracy;
    Power = @event.Power;
    PowerPoints = @event.PowerPoints;
  }

  public void Update(Name? name, Summary? summary, Content? content, ActorId? actorId = null)
  {
    if (!Equals(Name, name) || !Equals(Summary, summary) || !Equals(Content, content))
    {
      Raise(new MoveUpdated(name, summary, content), actorId);
    }
  }
  private void Handle(MoveUpdated @event)
  {
    Name = @event.Name;
    Summary = @event.Summary;
    Content = @event.Content;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
