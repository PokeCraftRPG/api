using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MoveEntity : AggregateEntity
{
  public int MoveId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }

  public string? Description { get; private set; }

  public byte? Accuracy { get; private set; }
  public byte? Power { get; private set; }
  public byte PowerPoints { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public MoveEntity(WorldEntity world, MoveCreated @event) : base(@event)
  {
    Id = new MoveId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    Type = @event.Type;
    Category = @event.Category;

    Key = @event.Key.Value;

    PowerPoints = @event.PowerPoints.Value;
  }

  private MoveEntity() : base()
  {
  }

  public void SetKey(MoveKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(MoveUpdated @event)
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

    if (@event.Accuracy is not null)
    {
      Accuracy = @event.Accuracy.Value?.Value;
    }
    if (@event.Power is not null)
    {
      Power = @event.Power.Value?.Value;
    }
    if (@event.PowerPoints is not null)
    {
      PowerPoints = @event.PowerPoints.Value;
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
