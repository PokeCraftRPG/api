using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;

namespace PokeGame.Infrastructure.Entities;

internal class AbilityEntity : AggregateEntity
{
  public int AbilityId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid EntityId { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public AbilityEntity(WorldEntity world, AbilityCreated @event) : base(@event)
  {
    World = world;
    WorldId = world.WorldId;
    EntityId = new AbilityId(@event.StreamId).EntityId;

    Key = @event.Key.Value;
  }

  private AbilityEntity() : base()
  {
  }

  public void Describe(AbilityDescribed @event)
  {
    base.Update(@event);

    Description = @event.Description?.Value;
  }

  public void Rename(AbilityRenamed @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
  }

  public void SetKey(AbilityKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
