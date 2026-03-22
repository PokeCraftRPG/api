using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Events;

namespace PokeGame.Infrastructure.Entities;

internal class MoveEntity : AggregateEntity
{
  public int MoveId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid WorldUid { get; private set; }
  public Guid Id { get; private set; }

  public PokemonType Type { get; private set; }
  public MoveCategory Category { get; private set; }

  public string Name { get; private set; } = string.Empty;
  public string? Description { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public MoveEntity(WorldEntity world, MoveCreated @event) : base(@event)
  {
    Id = new MoveId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;
    WorldUid = world.Id;

    Type = @event.Type;
    Category = @event.Category;

    Name = @event.Name.Value;
  }

  private MoveEntity() : base()
  {
  }

  public void Update(MoveUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
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

  public override string ToString() => $"{Name} | {base.ToString()}";
}
