using Logitar.EventSourcing;
using PokeGame.Core.Moves.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class Move : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Move";

  private MoveUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Description is not null
    || _updated.Accuracy is not null || _updated.Power is not null || _updated.PowerPoints is not null
    || _updated.Url is not null || _updated.Notes is not null;

  public new MoveId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The move was not initialized.");
  private Name? _name = null;
  public Name? Name
  {
    get => _name;
    set
    {
      if (_name != value)
      {
        _name = value;
        _updated.Name = new Optional<Name>(value);
      }
    }
  }
  private Description? _description = null;
  public Description? Description
  {
    get => _description;
    set
    {
      if (_description != value)
      {
        _description = value;
        _updated.Description = new Optional<Description>(value);
      }
    }
  }

  private Accuracy? _accuracy = null;
  public Accuracy? Accuracy
  {
    get => _accuracy;
    set
    {
      if (_accuracy != value)
      {
        _accuracy = value;
        _updated.Accuracy = new Optional<Accuracy>(value);
      }
    }
  }
  private Power? _power = null;
  public Power? Power
  {
    get => _power;
    set
    {
      if (value is not null && Category == MoveCategory.Status)
      {
        throw new StatusMoveCannotHavePowerException(this, value);
      }
      else if (_power != value)
      {
        _power = value;
        _updated.Power = new Optional<Power>(value);
      }
    }
  }
  private PowerPoints? _powerPoints = null;
  public PowerPoints PowerPoints
  {
    get => _powerPoints ?? throw new InvalidOperationException("The move was not initialized.");
    set
    {
      if (_powerPoints != value)
      {
        _powerPoints = value;
        _updated.PowerPoints = value;
      }
    }
  }

  private Url? _url = null;
  public Url? Url
  {
    get => _url;
    set
    {
      if (_url != value)
      {
        _url = value;
        _updated.Url = new Optional<Url>(value);
      }
    }
  }
  private Notes? _notes = null;
  public Notes? Notes
  {
    get => _notes;
    set
    {
      if (_notes != value)
      {
        _notes = value;
        _updated.Notes = new Optional<Notes>(value);
      }
    }
  }

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Move() : base()
  {
  }

  // TODO(fpion): should we add Accuracy and Power?

  public Move(World world, PokemonType type, MoveCategory category, Slug key, PowerPoints? powerPoints = null, UserId? userId = null)
    : this(type, category, key, powerPoints ?? new PowerPoints(PowerPoints.MinimumValue), userId ?? world.OwnerId, MoveId.NewId(world.Id))
  {
  }

  public Move(PokemonType type, MoveCategory category, Slug key, PowerPoints powerPoints, UserId userId, MoveId moveId)
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

    Raise(new MoveCreated(type, category, key, powerPoints), userId.ActorId);
  }
  protected virtual void Handle(MoveCreated @event)
  {
    Type = @event.Type;
    Category = @event.Category;

    _key = @event.Key;

    _powerPoints = @event.PowerPoints;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new MoveDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new MoveKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(MoveKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new();
    }
  }
  protected virtual void Handle(MoveUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }

    if (@event.Accuracy is not null)
    {
      _accuracy = @event.Accuracy.Value;
    }
    if (@event.Power is not null)
    {
      _power = @event.Power.Value;
    }
    if (@event.PowerPoints is not null)
    {
      _powerPoints = @event.PowerPoints;
    }

    if (@event.Url is not null)
    {
      _url = @event.Url.Value;
    }
    if (@event.Notes is not null)
    {
      _notes = @event.Notes.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
