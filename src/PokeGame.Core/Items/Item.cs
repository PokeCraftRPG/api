using Logitar.EventSourcing;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Items.Properties;
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

  public ItemCategory Category { get; private set; }

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

  private ItemProperties? _properties = null;
  public ItemProperties Properties => _properties ?? throw new InvalidOperationException("The item was not initialized.");

  public long Size => Key.Size + (Name?.Size ?? 0) + (Description?.Size ?? 0) + (Sprite?.Size ?? 0) + (Url?.Size ?? 0) + (Notes?.Size ?? 0);

  public Item() : base()
  {
  }

  public Item(World world, Slug key, ItemProperties properties, UserId? userId = null)
    : this(key, properties, userId ?? world.OwnerId, ItemId.NewId(world.Id))
  {
  }

  public Item(Slug key, ItemProperties properties, UserId userId, ItemId itemId)
    : base(itemId.StreamId)
  {
    if (!Enum.IsDefined(properties.Category))
    {
      throw new ArgumentOutOfRangeException(nameof(properties), "The item category is not defined.");
    }

    Raise(new ItemCreated(properties.Category, key), userId.ActorId);

    SetProperties(properties, userId);
  }
  protected virtual void Handle(ItemCreated @event)
  {
    Category = @event.Category;

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

  public void SetProperties(ItemProperties properties, UserId userId)
  {
    if (Category != properties.Category)
    {
      throw new ArgumentException($"Cannot set properties of category '{properties.Category}' on an item in category '{Category}'.", nameof(properties));
    }

    if (_properties != properties)
    {
      switch (properties.Category)
      {
        case ItemCategory.BattleItem:
          Raise(new BattleItemPropertiesChanged((BattleItemProperties)properties), userId.ActorId);
          break;
        case ItemCategory.Berry:
          Raise(new BerryPropertiesChanged((BerryProperties)properties), userId.ActorId);
          break;
        case ItemCategory.KeyItem:
          Raise(new KeyItemPropertiesChanged((KeyItemProperties)properties), userId.ActorId);
          break;
        case ItemCategory.Material:
          Raise(new MaterialPropertiesChanged((MaterialProperties)properties), userId.ActorId);
          break;
        case ItemCategory.Medicine:
          Raise(new MedicinePropertiesChanged((MedicineProperties)properties), userId.ActorId);
          break;
        case ItemCategory.OtherItem:
          Raise(new OtherItemPropertiesChanged((OtherItemProperties)properties), userId.ActorId);
          break;
        case ItemCategory.PokeBall:
          Raise(new PokeBallPropertiesChanged((PokeBallProperties)properties), userId.ActorId);
          break;
        case ItemCategory.TechnicalMachine:
          TechnicalMachineProperties technicalMachineProperties = (TechnicalMachineProperties)properties;
          WorldMismatchException.ThrowIfMismatch(Id, technicalMachineProperties.MoveId, nameof(properties));
          Raise(new TechnicalMachinePropertiesChanged(technicalMachineProperties), userId.ActorId);
          break;
        case ItemCategory.Treasure:
          Raise(new TreasurePropertiesChanged((TreasureProperties)properties), userId.ActorId);
          break;
        default:
          throw new ItemCategoryNotSupportedException(properties.Category);
      }
    }
  }
  protected virtual void Handle(BattleItemPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(BerryPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(KeyItemPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(MaterialPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(MedicinePropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(OtherItemPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(PokeBallPropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(TechnicalMachinePropertiesChanged @event)
  {
    _properties = @event.Properties;
  }
  protected virtual void Handle(TreasurePropertiesChanged @event)
  {
    _properties = @event.Properties;
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
