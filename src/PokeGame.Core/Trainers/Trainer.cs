using Logitar.EventSourcing;
using PokeGame.Core.Trainers.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public class Trainer : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Trainer";

  private TrainerUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Description is not null
    || _updated.Gender.HasValue || _updated.Money is not null
    || _updated.Sprite is not null || _updated.Url is not null || _updated.Notes is not null;

  public new TrainerId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  // TODO(fpion): License

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The trainer was not initialized.");
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

  private TrainerGender _gender;
  public TrainerGender Gender
  {
    get => _gender;
    set
    {
      if (!Enum.IsDefined(Gender))
      {
        throw new ArgumentOutOfRangeException(nameof(Gender));
      }
      else if (_gender != value)
      {
        _gender = value;
        _updated.Gender = value;
      }
    }
  }
  private Money _money = new();
  public Money Money
  {
    get => _money;
    set
    {
      if (_money != value)
      {
        _money = value;
        _updated.Money = value;
      }
    }
  }

  private Url? _sprite = null;
  public Url? Sprite
  {
    get => _sprite;
    set
    {
      if (_sprite != value)
      {
        _sprite = value;
        _updated.Sprite = new Optional<Url>(value);
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

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Sprite?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0); // TODO(fpion): License

  public Trainer() : base()
  {
  }

  public Trainer(World world, Slug key, TrainerGender gender, UserId? userId = null)
    : this(key, gender, userId ?? world.OwnerId, TrainerId.NewId(world.Id))
  {
  }

  public Trainer(Slug key, TrainerGender gender, UserId userId, TrainerId trainerId)
    : base(trainerId.StreamId)
  {
    Raise(new TrainerCreated(key, gender), userId.ActorId);
  }

  protected virtual void Handle(TrainerCreated @event)
  {
    _key = @event.Key;

    _gender = @event.Gender;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new TrainerDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new TrainerKeyChanged(key), userId.ActorId);
    }
  }

  protected virtual void Handle(TrainerKeyChanged @event)
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

  protected virtual void Handle(TrainerUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }

    if (@event.Gender.HasValue)
    {
      _gender = @event.Gender.Value;
    }
    if (@event.Money is not null)
    {
      _money = @event.Money;
    }

    if (@event.Sprite is not null)
    {
      _sprite = @event.Sprite.Value;
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
