using PokeGame.Core;
using PokeGame.Core.Abilities.Events;

namespace PokeGame.Infrastructure.Entities;

internal class AbilityEntity : AggregateEntity
{
  public int AbilityId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public AbilityEntity(int worldId, AbilityCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    Key = @event.Key.Value;
  }

  private AbilityEntity() : base()
  {
  }

  public void SetDetails(AbilityDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(AbilityKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
