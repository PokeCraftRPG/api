using PokeGame.Core.Abilities;
using PokeGame.Core.Abilities.Events;

namespace PokeGame.Infrastructure.Entities;

internal class AbilityEntity : AggregateEntity
{
  public int AbilityId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid WorldUid { get; private set; }
  public Guid Id { get; private set; }

  // TODO(fpion): Key/Slug
  public string? Name { get; private set; }
  public string? Description { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public AbilityEntity(WorldEntity world, AbilityCreated @event) : base(@event)
  {
    Id = new AbilityId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;
    WorldUid = world.Id;
  }

  private AbilityEntity() : base()
  {
  }

  public void Update(AbilityUpdated @event)
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

    if (@event.Url is not null)
    {
      Url = @event.Url.Value?.Value;
    }
    if (@event.Notes is not null)
    {
      Notes = @event.Notes.Value?.Value;
    }
  }

  // TODO(fpion): ToString
}
