using PokeGame.Core.Species;
using PokeGame.Core.Species.Events;

namespace PokeGame.Infrastructure.Entities;

internal class SpeciesEntity : AggregateEntity
{
  public int SpeciesId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public int Number { get; private set; }
  public PokemonCategory Category { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public SpeciesEntity(WorldEntity world, SpeciesCreated @event) : base(@event)
  {
    Id = new SpeciesId(@event.StreamId).EntityId;

    World = world;
    WorldId = world.WorldId;

    Number = @event.Number.Value;
    Category = @event.Category;

    Key = @event.Key.Value;
  }

  private SpeciesEntity() : base()
  {
  }

  public void SetKey(SpeciesKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(SpeciesUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
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
