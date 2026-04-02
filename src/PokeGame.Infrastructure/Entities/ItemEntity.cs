using PokeGame.Core.Items;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Items.Models;

namespace PokeGame.Infrastructure.Entities;

internal class ItemEntity : AggregateEntity
{
  public int ItemId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public ItemCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public int? Price { get; private set; }

  public string? Sprite { get; private set; }
  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public MoveEntity? Move { get; private set; }
  public int? MoveId { get; private set; }

  public string? Properties { get; private set; }

  public List<EvolutionEntity> Evolutions { get; private set; } = [];
  public List<EvolutionEntity> HeldEvolutions { get; private set; } = [];

  public ItemEntity(WorldEntity world, ItemCreated @event) : base(@event)
  {
    Id = new ItemId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private ItemEntity() : base()
  {
  }

  public void SetKey(ItemKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void SetProperties(BattleItemPropertiesChanged @event)
  {
    Update(@event);

    BattleItemPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(BerryPropertiesChanged @event)
  {
    Update(@event);

    BerryPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(KeyItemPropertiesChanged @event)
  {
    Update(@event);

    KeyItemPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(MaterialPropertiesChanged @event)
  {
    Update(@event);

    MaterialPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(MedicinePropertiesChanged @event)
  {
    Update(@event);

    MedicinePropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(OtherItemPropertiesChanged @event)
  {
    Update(@event);

    OtherItemPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(PokeBallPropertiesChanged @event)
  {
    Update(@event);

    PokeBallPropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }
  public void SetProperties(MoveEntity move, TechnicalMachinePropertiesChanged @event)
  {
    Update(@event);

    Move = move;
    MoveId = move.MoveId;

    Properties = null;
  }
  public void SetProperties(TreasurePropertiesChanged @event)
  {
    Update(@event);

    TreasurePropertiesModel properties = new(@event.Properties);
    Properties = PokemonSerializer.Instance.Serialize(properties);
  }

  public void Update(ItemUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }

    if (@event.Price is not null)
    {
      Price = @event.Price.Value?.Value;
    }

    if (@event.Sprite is not null)
    {
      Sprite = @event.Sprite.Value?.Value;
    }
    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
