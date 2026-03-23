using Logitar.EventSourcing;
using PokeGame.Core.Species.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class SpeciesAggregate : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Species";

  private SpeciesUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null;

  public new SpeciesId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

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

  public long Size => Key.Size + (Name?.Size ?? 0);

  public SpeciesAggregate() : base()
  {
  }

  public SpeciesAggregate(World world, PokemonCategory category, Slug key, UserId? userId = null)
    : this(category, key, userId ?? world.OwnerId, SpeciesId.NewId(world.Id))
  {
  }

  public SpeciesAggregate(PokemonCategory category, Slug key, UserId userId, SpeciesId speciesId)
    : base(speciesId.StreamId)
  {
    if (!Enum.IsDefined(category))
    {
      throw new ArgumentOutOfRangeException(nameof(category));
    }

    Raise(new SpeciesCreated(category, key), userId.ActorId);
  }

  protected virtual void Handle(SpeciesCreated @event)
  {
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
  }

  public override string ToString() => $"{Name?.Value ?? Key.Value} | {base.ToString()}";
}
