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
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public int? Accuracy { get; private set; }
  public int? Power { get; private set; }
  public int? PowerPoints { get; private set; }

  public List<VarietyMoveEntity> Varieties { get; private set; } = [];

  public MoveEntity(int worldId, MoveCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    Type = @event.Type;
    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private MoveEntity() : base()
  {
  }

  public void SetKey(MoveKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetMechanics(MoveMechanicsChanged @event)
  {
    Update(@event);

    Accuracy = @event.Accuracy?.Value;
    Power = @event.Power?.Value;
    PowerPoints = @event.PowerPoints?.Value;
  }

  public void Update(MoveUpdated @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
