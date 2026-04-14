using Logitar.EventSourcing;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class PokemonSpecies : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  private SpeciesUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null
    || _updated.BaseFriendship is not null || _updated.CatchRate is not null || _updated.GrowthRate.HasValue
    || _updated.EggCycles is not null || _updated.EggGroups is not null
    || _updated.Url is not null || _updated.Notes is not null;

  public new SpeciesId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Number? _number = null;
  public Number Number => _number ?? throw new InvalidOperationException("The species was not initialized.");
  public PokemonCategory Category { get; private set; }

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The species was not initialized.");
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

  private Friendship? _baseFriendship = null;
  public Friendship BaseFriendship
  {
    get => _baseFriendship ?? throw new InvalidOperationException("The species was not initialized.");
    set
    {
      if (_baseFriendship != value)
      {
        _baseFriendship = value;
        _updated.BaseFriendship = value;
      }
    }
  }
  private CatchRate? _catchRate = null;
  public CatchRate CatchRate
  {
    get => _catchRate ?? throw new InvalidOperationException("The species was not initialized.");
    set
    {
      if (_catchRate != value)
      {
        _catchRate = value;
        _updated.CatchRate = value;
      }
    }
  }
  private GrowthRate _growthRate = default;
  public GrowthRate GrowthRate
  {
    get => _growthRate;
    set
    {
      if (_growthRate != value)
      {
        _growthRate = value;
        _updated.GrowthRate = value;
      }
    }
  }

  private EggCycles? _eggCycles = null;
  public EggCycles EggCycles
  {
    get => _eggCycles ?? throw new InvalidOperationException("The species was not initialized.");
    set
    {
      if (_eggCycles != value)
      {
        _eggCycles = value;
        _updated.EggCycles = value;
      }
    }
  }
  private EggGroups? _eggGroups = null;
  public EggGroups EggGroups
  {
    get => _eggGroups ?? throw new InvalidOperationException("The species was not initialized.");
    set
    {
      if (_eggGroups != value)
      {
        _eggGroups = value;
        _updated.EggGroups = value;
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

  private readonly Dictionary<RegionId, Number> _regionalNumbers = [];
  public IReadOnlyDictionary<RegionId, Number> RegionalNumbers => _regionalNumbers.AsReadOnly();

  public long Size => Key.Size + (Name?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public PokemonSpecies() : base()
  {
  }

  public PokemonSpecies(
    World world,
    Number number,
    PokemonCategory category,
    Slug key,
    Friendship baseFriendship,
    CatchRate catchRate,
    GrowthRate growthRate,
    EggCycles eggCycles,
    EggGroups eggGroups,
    UserId? userId = null) : this(number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups, userId ?? world.OwnerId, SpeciesId.NewId(world.Id))
  {
  }

  public PokemonSpecies(
    Number number,
    PokemonCategory category,
    Slug key,
    Friendship baseFriendship,
    CatchRate catchRate,
    GrowthRate growthRate,
    EggCycles eggCycles,
    EggGroups eggGroups,
    UserId userId,
    SpeciesId speciesId) : base(speciesId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }
    if (!Enum.IsDefined(growthRate))
    {
      throw new ArgumentOutOfRangeException(nameof(growthRate));
    }

    Raise(new SpeciesCreated(number, category, key, baseFriendship, catchRate, growthRate, eggCycles, eggGroups), userId.ActorId);
  }
  protected virtual void Handle(SpeciesCreated @event)
  {
    _number = @event.Number;
    Category = @event.Category;

    _key = @event.Key;

    _baseFriendship = @event.BaseFriendship;
    _catchRate = @event.CatchRate;
    _growthRate = @event.GrowthRate;

    _eggCycles = @event.EggCycles;
    _eggGroups = @event.EggGroups;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new SpeciesDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new SpeciesKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(SpeciesKeyChanged @event)
  {
    _key = @event.Key;
  }

  public void SetRegionalNumber(Region region, Number? number, UserId userId) => SetRegionalNumber(region.Id, number, userId);
  public void SetRegionalNumber(RegionId regionId, Number? number, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, regionId, nameof(regionId));

    _regionalNumbers.TryGetValue(regionId, out Number? existingNumber);
    if (existingNumber != number)
    {
      Raise(new SpeciesRegionalNumberChanged(regionId, number), userId.ActorId);
    }
  }
  protected virtual void Handle(SpeciesRegionalNumberChanged @event)
  {
    if (@event.Number is null)
    {
      _regionalNumbers.Remove(@event.RegionId);
    }
    else
    {
      _regionalNumbers[@event.RegionId] = @event.Number;
    }
  }

  public void Update(UserId userId)
  {
    if (HasUpdates)
    {
      Raise(_updated, userId.ActorId, DateTime.Now);
      _updated = new();
    }
  }

  protected virtual void Handle(SpeciesUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }

    if (@event.BaseFriendship is not null)
    {
      _baseFriendship = @event.BaseFriendship;
    }
    if (@event.CatchRate is not null)
    {
      _catchRate = @event.CatchRate;
    }
    if (@event.GrowthRate.HasValue)
    {
      _growthRate = @event.GrowthRate.Value;
    }

    if (@event.EggCycles is not null)
    {
      _eggCycles = @event.EggCycles;
    }
    if (@event.EggGroups is not null)
    {
      _eggGroups = @event.EggGroups;
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
