using PokeGame.Core;
using PokeGame.Core.Regions.Events;

namespace PokeGame.Infrastructure.Entities;

internal class RegionEntity : AggregateEntity
{
  public int RegionId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string Key { get; private set; } = string.Empty;

  public string? Name { get; private set; }
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public List<RegionalNumberEntity> RegionalNumbers { get; private set; } = [];

  public RegionEntity(int worldId, RegionCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    Key = @event.Key.Value;
  }

  private RegionEntity() : base()
  {
  }

  public void SetDetails(RegionDetailsChanged @event)
  {
    Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;
  }

  public void SetKey(RegionKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
