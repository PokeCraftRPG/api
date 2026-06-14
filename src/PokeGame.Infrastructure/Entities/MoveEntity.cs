using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MoveEntity : AggregateEntity
{
  public int MoveId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid EntityId { get; private set; }

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public MoveEntity(WorldEntity world, MoveCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    EntityId = new MoveId(@event.StreamId).EntityId;

    Type = @event.Type;
    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private MoveEntity() : base()
  {
  }

  public void Describe(MoveDescribed @event)
  {
    base.Update(@event);

    Description = @event.Description?.Value;
  }

  public void Rename(MoveRenamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void SetKey(MoveKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
