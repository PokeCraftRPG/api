using Logitar.EventSourcing;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class SpeciesAggregate : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  private SpeciesUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null
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

  public long Size => Key.Size + (Name?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public SpeciesAggregate() : base()
  {
  }

  public SpeciesAggregate(World world, Number number, PokemonCategory category, Slug key, UserId? userId = null)
    : this(number, category, key, userId ?? world.OwnerId, SpeciesId.NewId(world.Id))
  {
  }

  public SpeciesAggregate(Number number, PokemonCategory category, Slug key, UserId userId, SpeciesId speciesId)
    : base(speciesId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new SpeciesCreated(number, category, key), userId.ActorId);
  }
  protected virtual void Handle(SpeciesCreated @event)
  {
    _number = @event.Number;
    Category = @event.Category;

    _key = @event.Key;
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
