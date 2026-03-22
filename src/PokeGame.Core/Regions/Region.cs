using Logitar.EventSourcing;
using PokeGame.Core.Regions.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public class Region : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Region";

  private RegionUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Description is not null
    || _updated.Url is not null || _updated.Notes is not null;

  public new RegionId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Name? _name = null;
  public Name Name
  {
    get => _name ?? throw new InvalidOperationException("The region was not initialized.");
    set
    {
      if (_name != value)
      {
        _name = value;
        _updated.Name = value;
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

  public long Size => Name.Size + (Description?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Region() : base()
  {
  }

  public Region(World world, Name name, UserId? userId = null)
    : this(name, userId ?? world.OwnerId, RegionId.NewId(world.Id))
  {
  }

  public Region(Name name, UserId userId, RegionId regionId)
    : base(regionId.StreamId)
  {
    Raise(new RegionCreated(name), userId.ActorId);
  }
  protected virtual void Handle(RegionCreated @event)
  {
    _name = @event.Name;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new RegionDeleted(), userId.ActorId);
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
  protected virtual void Handle(RegionUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
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

  public override string ToString() => $"{Name} | {base.ToString()}";
}
