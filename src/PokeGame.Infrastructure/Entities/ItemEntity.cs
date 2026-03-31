using PokeGame.Core.Items;
using PokeGame.Core.Items.Events;

namespace PokeGame.Infrastructure.Entities;

internal class ItemEntity : AggregateEntity
{
  public int ItemId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public int? Price { get; private set; }

  public string? Sprite { get; private set; }
  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public ItemEntity(WorldEntity world, ItemCreated @event) : base(@event)
  {
    Id = new ItemId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

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
