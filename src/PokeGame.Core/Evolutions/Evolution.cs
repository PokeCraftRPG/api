using Logitar.EventSourcing;
using PokeGame.Core.Evolutions.Events;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

public class Evolution : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Evolution";

  private EvolutionUpdated _updated = new();
  public bool HasUpdates => _updated.Level is not null || _updated.Friendship.HasValue || _updated.Gender is not null
    || _updated.HeldItemId is not null || _updated.KnownMoveId is not null || _updated.Location is not null || _updated.TimeOfDay is not null;

  public new EvolutionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public FormId SourceId { get; private set; }
  public FormId TargetId { get; private set; }

  public EvolutionTrigger Trigger { get; private set; }
  public ItemId? ItemId { get; private set; }

  private Level? _level = null;
  public Level? Level
  {
    get => _level;
    set
    {
      if (_level != value)
      {
        _level = value;
        _updated.Level = new Optional<Level>(value);
      }
    }
  }
  private bool _friendship = false;
  public bool Friendship
  {
    get => _friendship;
    set
    {
      if (_friendship != value)
      {
        _friendship = value;
        _updated.Friendship = value;
      }
    }
  }
  private PokemonGender? _gender = null;
  public PokemonGender? Gender
  {
    get => _gender;
    set
    {
      if (_gender != value)
      {
        _gender = value;
        _updated.Gender = new Optional<PokemonGender?>(value);
      }
    }
  }
  private ItemId? _heldItemId = null;
  public ItemId? HeldItemId
  {
    get => _heldItemId;
    set
    {
      if (value.HasValue)
      {
        WorldMismatchException.ThrowIfMismatch(Id, value.Value, nameof(HeldItemId));
      }

      if (_heldItemId != value)
      {
        _heldItemId = value;
        _updated.HeldItemId = new Optional<ItemId?>(value);
      }
    }
  }
  private MoveId? _knownMoveId = null;
  public MoveId? KnownMoveId
  {
    get => _knownMoveId;
    set
    {
      if (value.HasValue)
      {
        WorldMismatchException.ThrowIfMismatch(Id, value.Value, nameof(KnownMoveId));
      }

      if (_knownMoveId != value)
      {
        _knownMoveId = value;
        _updated.KnownMoveId = new Optional<MoveId?>(value);
      }
    }
  }
  private Location? _location = null;
  public Location? Location
  {
    get => _location;
    set
    {
      if (_location != value)
      {
        _location = value;
        _updated.Location = new Optional<Location>(value);
      }
    }
  }
  private TimeOfDay? _timeOfDay = null;
  public TimeOfDay? TimeOfDay
  {
    get => _timeOfDay;
    set
    {
      if (_timeOfDay != value)
      {
        _timeOfDay = value;
        _updated.TimeOfDay = new Optional<TimeOfDay?>(value);
      }
    }
  }

  public long Size => Location?.Size ?? 0;

  public Evolution() : base()
  {
  }

  public Evolution(World world, Form source, Form target, EvolutionTrigger trigger, Item? item)
    : this(source, target, trigger, item, world.OwnerId, EvolutionId.NewId(world.Id))
  {
  }

  public Evolution(Form source, Form target, EvolutionTrigger trigger, Item? item, UserId userId, EvolutionId evolutionId)
    : base(evolutionId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, source, nameof(source));
    WorldMismatchException.ThrowIfMismatch(Id, target, nameof(target));

    if (item is not null)
    {
      WorldMismatchException.ThrowIfMismatch(Id, item, nameof(item));
    }

    if (source.Equals(target) || source.VarietyId == target.VarietyId)
    {
      throw new NotImplementedException(); // TODO(fpion): implement
    }

    if (!Enum.IsDefined(trigger))
    {
      throw new ArgumentOutOfRangeException(nameof(trigger));
    }

    if (trigger == EvolutionTrigger.Item && item is null)
    {
      throw new ArgumentNullException(nameof(item), "The item should not be null when the evolution is triggered by an item.");
    }
    else if (trigger != EvolutionTrigger.Item && item is not null)
    {
      throw new ArgumentException("The item should be null when the evolution is not triggered by an item.", nameof(item));
    }

    Raise(new EvolutionCreated(source.Id, target.Id, trigger, item?.Id), userId.ActorId);
  }
  protected virtual void Handle(EvolutionCreated @event)
  {
    SourceId = @event.SourceId;
    TargetId = @event.TargetId;

    Trigger = @event.Trigger;
    ItemId = @event.ItemId;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new EvolutionDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new();
    }
  }
  protected virtual void Handle(EvolutionUpdated @event)
  {
    if (@event.Level is not null)
    {
      _level = @event.Level.Value;
    }
    if (@event.Friendship.HasValue)
    {
      _friendship = @event.Friendship.Value;
    }
    if (@event.Gender is not null)
    {
      _gender = @event.Gender.Value;
    }
    if (@event.HeldItemId is not null)
    {
      _heldItemId = @event.HeldItemId.Value;
    }
    if (@event.KnownMoveId is not null)
    {
      _knownMoveId = @event.KnownMoveId.Value;
    }
    if (@event.Location is not null)
    {
      _location = @event.Location.Value;
    }
    if (@event.TimeOfDay is not null)
    {
      _timeOfDay = @event.TimeOfDay.Value;
    }
  }
}
