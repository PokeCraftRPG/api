using Logitar.EventSourcing;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class Move : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Move";

  public new MoveId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The key was not initialized.");
  public Name? Name { get; private set; }
  public Description? Description { get; private set; }

  public Accuracy? Accuracy { get; private set; }
  public Power? Power { get; private set; }
  private PowerPoints? _powerPoints = null;
  public PowerPoints PowerPoints => _powerPoints ?? throw new InvalidOperationException("The power points were not initialized.");

  public Move() : base()
  {
  }

  public Move(World world, PokemonType type, MoveCategory category, Slug key, PowerPoints powerPoints, Accuracy? accuracy = null, Power? power = null, ActorId? actorId = null)
    : this(MoveId.NewId(world.Id), type, category, key, powerPoints, accuracy, power, actorId)
  {
  }

  public Move(MoveId moveId, PokemonType type, MoveCategory category, Slug key, PowerPoints powerPoints, Accuracy? accuracy = null, Power? power = null, ActorId? actorId = null)
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

    // TODO(fpion): status moves should not have Power!

    Raise(new MoveCreated(type, category, key, accuracy, power, powerPoints), actorId);
  }
  protected virtual void Handle(MoveCreated @event)
  {
    Type = @event.Type;
    Category = @event.Category;

    _key = @event.Key;

    Accuracy = @event.Accuracy;
    Power = @event.Power;
    _powerPoints = @event.PowerPoints;
  }

  public void Delete(ActorId? actorId = null)
  {
    if (!IsDeleted)
    {
      Raise(new MoveDeleted(), actorId);
    }
  }

  public void Describe(Description? description, ActorId? actorId = null)
  {
    if (!Equals(Description, description))
    {
      Raise(new MoveDescribed(description), actorId);
    }
  }
  protected virtual void Handle(MoveDescribed @event)
  {
    Description = @event.Description;
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId);

  public void Rename(Name? name, ActorId? actorId = null)
  {
    if (!Equals(Name, name))
    {
      Raise(new MoveRenamed(name), actorId);
    }
  }
  protected virtual void Handle(MoveRenamed @event)
  {
    Name = @event.Name;
  }

  public void SetGameData(Accuracy? accuracy, Power? power, PowerPoints powerPoints, ActorId? actorId = null)
  {
    if (!Equals(Accuracy, accuracy) || !Equals(Power, power) || !Equals(PowerPoints, powerPoints))
    {
      // TODO(fpion): status moves should not have Power!
      Raise(new MoveGameDataChanged(accuracy, power, powerPoints), actorId);
    }
  }
  protected virtual void Handle(MoveGameDataChanged @event)
  {
    Accuracy = @event.Accuracy;
    Power = @event.Power;
    _powerPoints = @event.PowerPoints;
  }

  public void SetKey(Slug key, ActorId? actorId = null)
  {
    if (!Equals(Key, key))
    {
      Raise(new MoveKeyChanged(key), actorId);
    }
  }
  protected virtual void Handle(MoveKeyChanged @event)
  {
    _key = @event.Key;
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
