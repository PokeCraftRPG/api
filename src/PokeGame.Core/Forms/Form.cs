using Logitar.EventSourcing;
using PokeGame.Core.Forms.Events;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Forms;

public class Form : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Form";

  private FormUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Description is not null
    || _updated.IsBattleOnly.HasValue || _updated.IsMega.HasValue
    || _updated.Height is not null || _updated.Weight is not null
    || _updated.Types is not null // TODO(fpion): Abilities, BaseStatistics, Yield, Sprites
    || _updated.Url is not null || _updated.Note is not null;

  public new FormId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public VarietyId VarietyId { get; private set; }
  public bool IsDefault { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The form was not initialized.");
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

  private bool _isBattleOnly = false;
  public bool IsBattleOnly
  {
    get => _isBattleOnly;
    set
    {
      if (_isBattleOnly != value)
      {
        _isBattleOnly = value;
        _updated.IsBattleOnly = value;
      }
    }
  }

  private bool _isMega = false;
  public bool IsMega
  {
    get => _isMega;
    set
    {
      if (_isMega != value)
      {
        _isMega = value;
        _updated.IsMega = value;
      }
    }
  }

  private Height? _height = null;
  public Height Height
  {
    get => _height ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_height != value)
      {
        _height = value;
        _updated.Height = value;
      }
    }
  }

  private Weight? _weight = null;
  public Weight Weight
  {
    get => _weight ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_weight != value)
      {
        _weight = value;
        _updated.Weight = value;
      }
    }
  }

  private FormTypes? _types = null;
  public FormTypes Types
  {
    get => _types ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_types != value)
      {
        _types = value;
        _updated.Types = value;
      }
    }
  }
  // TODO(fpion): Abilities
  // TODO(fpion): BaseStatistics
  // TODO(fpion): Yield
  // TODO(fpion): Sprites

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
  private Notes? _note = null;
  public Notes? Note
  {
    get => _note;
    set
    {
      if (_note != value)
      {
        _note = value;
        _updated.Note = new Optional<Notes>(value);
      }
    }
  }

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Url?.Size ?? 0) + (Note?.Size ?? 0);

  public Form() : base()
  {
  }

  public Form(World world, Variety variety, bool isDefault, Slug key, UserId? userId = null)
    : this(variety, isDefault, key, userId ?? world.OwnerId, FormId.NewId(world.Id))
  {
  }

  public Form(Variety variety, bool isDefault, Slug key, UserId userId, FormId formId)
    : base(formId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, variety, nameof(variety));

    Raise(new FormCreated(variety.Id, isDefault, key), userId.ActorId);
  }
  protected virtual void Handle(FormCreated @event)
  {
    VarietyId = @event.VarietyId;
    IsDefault = @event.IsDefault;

    _key = @event.Key;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new FormDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void SetDefault(UserId userId) => SetDefault(isDefault: true, userId);
  public void SetDefault(bool isDefault, UserId userId)
  {
    if (IsDefault != isDefault)
    {
      Raise(new FormDefaultChanged(isDefault), userId.ActorId);
    }
  }
  protected virtual void Handle(FormDefaultChanged @event)
  {
    IsDefault = @event.IsDefault;
  }

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new FormKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(FormKeyChanged @event)
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
  protected virtual void Handle(FormUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }

    if (@event.IsBattleOnly.HasValue)
    {
      _isBattleOnly = @event.IsBattleOnly.Value;
    }
    if (@event.IsMega.HasValue)
    {
      _isMega = @event.IsMega.Value;
    }

    if (@event.Height is not null)
    {
      _height = @event.Height;
    }
    if (@event.Weight is not null)
    {
      _weight = @event.Weight;
    }

    if (@event.Types is not null)
    {
      _types = @event.Types;
    }
    // TODO(fpion): Abilities
    // TODO(fpion): BaseStatistics
    // TODO(fpion): Yield
    // TODO(fpion): Sprites

    if (@event.Url is not null)
    {
      _url = @event.Url.Value;
    }
    if (@event.Note is not null)
    {
      _note = @event.Note.Value;
    }
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
