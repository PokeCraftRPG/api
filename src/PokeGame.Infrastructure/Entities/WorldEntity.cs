using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;

namespace PokeGame.Infrastructure.Entities;

internal class WorldEntity : AggregateEntity
{
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public string OwnerId { get; private set; } = string.Empty;

  public string Slug { get; private set; } = string.Empty;
  public string SlugNormalized
  {
    get => Core.Slug.Normalize(Slug);
    private set { }
  }
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public List<AbilityEntity> Abilities { get; private set; } = [];

  public WorldEntity(WorldCreated @event) : base(@event)
  {
    Id = new WorldId(@event.StreamId).ToGuid();

    OwnerId = @event.OwnerId.Value;

    Slug = @event.Slug.Value;
  }

  private WorldEntity() : base()
  {
  }

  public void Update(WorldUpdated @event)
  {
    base.Update(@event);

    if (@event.Slug is not null)
    {
      Slug = @event.Slug.Value;
    }
    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }
  }

  public override string ToString() => $"{Name ?? Slug} | {base.ToString()}";
}
