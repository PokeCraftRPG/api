using Logitar.EventSourcing;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public class Variety : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Variety";

  private VarietyUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Genus is not null || _updated.Description is not null
    || _updated.GenderRatio is not null || _updated.CanChangeForm.HasValue
    || _updated.Url is not null || _updated.Notes is not null;

  public new VarietyId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  public SpeciesId SpeciesId { get; private set; }
  public bool IsDefault { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The variety was not initialized.");
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
  private Genus? _genus = null;
  public Genus? Genus
  {
    get => _genus;
    set
    {
      if (_genus != value)
      {
        _genus = value;
        _updated.Genus = new Optional<Genus>(value);
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

  private GenderRatio? _genderRatio;
  public GenderRatio? GenderRatio
  {
    get => _genderRatio;
    set
    {
      if (_genderRatio != value)
      {
        _genderRatio = value;
        _updated.GenderRatio = new Optional<GenderRatio>(value);
      }
    }
  }

  private bool _canChangeForm = false;
  public bool CanChangeForm
  {
    get => _canChangeForm;
    set
    {
      if (_canChangeForm != value)
      {
        _canChangeForm = value;
        _updated.CanChangeForm = value;
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

  private readonly Dictionary<MoveId, Level?> _moves = [];
  public IReadOnlyDictionary<MoveId, Level?> AllMoves => _moves.AsReadOnly();
  public IReadOnlyCollection<MoveId> EvolutionMoves => _moves.Where(x => x.Value is null).Select(x => x.Key).ToList().AsReadOnly();
  public IReadOnlyDictionary<MoveId, Level> LevelMoves => _moves.Where(x => x.Value is not null).ToDictionary(x => x.Key, x => x.Value!).AsReadOnly();

  public long Size => Key.Size + (Name?.Size ?? 0) + (Genus?.Size ?? 0) + (Description?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Variety() : base()
  {
  }

  public Variety(World world, SpeciesAggregate species, bool isDefault, Slug key, UserId? userId = null)
    : this(species, isDefault, key, userId ?? world.OwnerId, VarietyId.NewId(world.Id))
  {
  }

  public Variety(SpeciesAggregate species, bool isDefault, Slug key, UserId userId, VarietyId varietyId)
    : base(varietyId.StreamId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, species, nameof(species));

    Raise(new VarietyCreated(species.Id, isDefault, key), userId.ActorId);
  }
  protected virtual void Handle(VarietyCreated @event)
  {
    SpeciesId = @event.SpeciesId;
    IsDefault = @event.IsDefault;

    _key = @event.Key;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new VarietyDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public bool RemoveMove(Move move, UserId userId) => RemoveMove(move.Id, userId);
  public bool RemoveMove(MoveId moveId, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, moveId, nameof(moveId));

    if (!_moves.ContainsKey(moveId))
    {
      return false;
    }

    Raise(new VarietyMoveRemoved(moveId), userId.ActorId);
    return true;
  }
  protected virtual void Handle(VarietyMoveRemoved @event)
  {
    _moves.Remove(@event.MoveId);
  }

  public void SetDefault(UserId userId) => SetDefault(isDefault: true, userId);
  public void SetDefault(bool isDefault, UserId userId)
  {
    if (IsDefault != isDefault)
    {
      Raise(new VarietyDefaultChanged(isDefault), userId.ActorId);
    }
  }
  protected virtual void Handle(VarietyDefaultChanged @event)
  {
    IsDefault = @event.IsDefault;
  }

  public void SetEvolutionMove(Move move, UserId userId) => SetEvolutionMove(move.Id, userId);
  public void SetEvolutionMove(MoveId moveId, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, moveId, nameof(moveId));

    if (!_moves.TryGetValue(moveId, out Level? level) || level is not null)
    {
      Raise(new VarietyEvolutionMoveChanged(moveId), userId.ActorId);
    }
  }
  protected virtual void Handle(VarietyEvolutionMoveChanged @event)
  {
    _moves[@event.MoveId] = null;
  }

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new VarietyKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(VarietyKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetLevelMove(Move move, Level level, UserId userId) => SetLevelMove(move.Id, level, userId);
  public void SetLevelMove(MoveId moveId, Level level, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, moveId, nameof(moveId));

    if (!_moves.TryGetValue(moveId, out Level? existingLevel) || existingLevel != level)
    {
      Raise(new VarietyLevelMoveChanged(moveId, level), userId.ActorId);
    }
  }
  protected virtual void Handle(VarietyLevelMoveChanged @event)
  {
    _moves[@event.MoveId] = @event.Level;
  }

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new();
    }
  }
  protected virtual void Handle(VarietyUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Genus is not null)
    {
      _genus = @event.Genus.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }

    if (@event.GenderRatio is not null)
    {
      _genderRatio = @event.GenderRatio.Value;
    }

    if (@event.CanChangeForm.HasValue)
    {
      _canChangeForm = @event.CanChangeForm.Value;
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
