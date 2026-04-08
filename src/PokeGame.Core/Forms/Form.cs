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
    || _updated.Types is not null || _updated.Abilities is not null || _updated.BaseStatistics is not null || _updated.Yield is not null || _updated.Sprites is not null
    || _updated.Url is not null || _updated.Notes is not null;

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
  private FormAbilities? _abilities = null;
  public FormAbilities Abilities
  {
    get => _abilities ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      ThrowIfMismatch(value, nameof(Abilities));
      if (_abilities != value)
      {
        _abilities = value;
        _updated.Abilities = value;
      }
    }
  }
  private BaseStatistics? _baseStatistics = null;
  public BaseStatistics BaseStatistics
  {
    get => _baseStatistics ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_baseStatistics != value)
      {
        _baseStatistics = value;
        _updated.BaseStatistics = value;
      }
    }
  }
  private Yield? _yield = null;
  public Yield Yield
  {
    get => _yield ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_yield != value)
      {
        _yield = value;
        _updated.Yield = value;
      }
    }
  }
  private Sprites? _sprites = null;
  public Sprites Sprites
  {
    get => _sprites ?? throw new InvalidOperationException("The form was not initialized.");
    set
    {
      if (_sprites != value)
      {
        _sprites = value;
        _updated.Sprites = value;
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

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0) + Sprites.CalculateSize();

  public Form() : base()
  {
  }

  public Form(
    World world,
    Variety variety,
    bool isDefault,
    Slug key,
    Height height,
    Weight weight,
    FormTypes types,
    FormAbilities abilities,
    BaseStatistics baseStatistics,
    Yield yield,
    Sprites sprites,
    UserId? userId = null) : this(variety, isDefault, key, height, weight, types, abilities, baseStatistics, yield, sprites, userId ?? world.OwnerId, FormId.NewId(world.Id))
  {
  }

  public Form(
    Variety variety,
    bool isDefault,
    Slug key,
    Height height,
    Weight weight,
    FormTypes types,
    FormAbilities abilities,
    BaseStatistics baseStatistics,
    Yield yield,
    Sprites sprites,
    UserId userId,
    FormId formId) : base(formId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, variety, nameof(variety));
    ThrowIfMismatch(abilities, nameof(abilities));

    Raise(new FormCreated(variety.Id, isDefault, key, height, weight, types, abilities, baseStatistics, yield, sprites), userId.ActorId);
  }
  protected virtual void Handle(FormCreated @event)
  {
    VarietyId = @event.VarietyId;
    IsDefault = @event.IsDefault;

    _key = @event.Key;

    _height = @event.Height;
    _weight = @event.Weight;

    _types = @event.Types;
    _abilities = @event.Abilities;
    _baseStatistics = @event.BaseStatistics;
    _yield = @event.Yield;
    _sprites = @event.Sprites;
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
    if (@event.Abilities is not null)
    {
      _abilities = @event.Abilities;
    }
    if (@event.BaseStatistics is not null)
    {
      _baseStatistics = @event.BaseStatistics;
    }
    if (@event.Yield is not null)
    {
      _yield = @event.Yield;
    }
    if (@event.Sprites is not null)
    {
      _sprites = @event.Sprites;
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

  private void ThrowIfMismatch(FormAbilities abilities, string paramName)
  {
    List<IEntityProvider> mismatched = new(capacity: 3);
    if (abilities.Primary.WorldId != WorldId)
    {
      mismatched.Add(abilities.Primary);
    }
    if (abilities.Secondary.HasValue && abilities.Secondary.Value.WorldId != WorldId)
    {
      mismatched.Add(abilities.Secondary.Value);
    }
    if (abilities.Hidden.HasValue && abilities.Hidden.Value.WorldId != WorldId)
    {
      mismatched.Add(abilities.Hidden.Value);
    }

    if (mismatched.Count > 0)
    {
      throw new WorldMismatchException(Id, mismatched, paramName);
    }
  }
}
