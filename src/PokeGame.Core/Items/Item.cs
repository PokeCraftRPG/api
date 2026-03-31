using Logitar.EventSourcing;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public class Item : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Item";

  private ItemUpdated _updated = new();
  public bool HasUpdates => _updated.Name is not null || _updated.Description is not null
    || _updated.Price is not null
    || _updated.Sprite is not null || _updated.Url is not null || _updated.Notes is not null;

  public new ItemId Id => new(base.Id);
  public WorldId WorldId => Id.WorldId;
  public Guid EntityId => Id.EntityId;

  private Slug? _key = null;
  public Slug Key => _key ?? throw new InvalidOperationException("The item was not initialized.");
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

  private Price? _price = null;
  public Price? Price
  {
    get => _price;
    set
    {
      if (_price != value)
      {
        _price = value;
        _updated.Price = new Optional<Price>(value);
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

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Sprite?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Item() : base()
  {
  }

  public Item(World world, Slug key, UserId? userId = null)
    : this(key, userId ?? world.OwnerId, ItemId.NewId(world.Id))
  {
  }

  public Item(Slug key, UserId userId, ItemId itemId)
    : base(itemId.StreamId)
  {
    Raise(new ItemCreated(key), userId.ActorId);
  }
  protected virtual void Handle(ItemCreated @event)
  {
    _key = @event.Key;
  }

  public void Delete(UserId userId)
  {
    if (!IsDeleted)
    {
      Raise(new ItemDeleted(), userId.ActorId);
    }
  }

  public Entity GetEntity() => new(EntityKind, EntityId, WorldId, Size);

  public void SetKey(Slug key, UserId userId)
  {
    if (_key != key)
    {
      Raise(new ItemKeyChanged(key), userId.ActorId);
    }
  }
  protected virtual void Handle(ItemKeyChanged @event)
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
  protected virtual void Handle(ItemUpdated @event)
  {
    if (@event.Name is not null)
    {
      _name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      _description = @event.Description.Value;
    }

    if (@event.Price is not null)
    {
      _price = @event.Price.Value;
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
