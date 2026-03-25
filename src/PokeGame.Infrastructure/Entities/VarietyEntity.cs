using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Varieties;
using PokeGame.Core.Varieties.Events;

namespace PokeGame.Infrastructure.Entities;

internal class VarietyEntity : AggregateEntity
{
  public int VarietyId { get; private set; }

  public WorldEntity? World { get; private set; }
  public int WorldId { get; private set; }
  public Guid Id { get; private set; }

  public SpeciesEntity? Species { get; private set; }
  public int SpeciesId { get; private set; }
  public bool IsDefault { get; private set; }

  public string Key { get; private set; } = string.Empty;
  public string? Name { get; private set; }
  public string? Genus { get; private set; }
  public string? Description { get; private set; }

  public int? GenderRatio { get; private set; }

  public bool CanChangeForm { get; private set; }

  public string? Url { get; private set; }
  public string? Notes { get; private set; }

  public List<VarietyMoveEntity> Moves { get; private set; } = [];

  public VarietyEntity(SpeciesEntity species, VarietyCreated @event) : base(@event)
  {
    Id = new VarietyId(@event.StreamId).EntityId;

    World = species.World;
    WorldId = species.WorldId;

    Species = species;
    SpeciesId = species.SpeciesId;
    IsDefault = @event.IsDefault;

    Key = @event.Key.Value;
  }

  private VarietyEntity() : base()
  {
  }

  public override IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = new(base.GetActorIds());
    if (Species is not null)
    {
      actorIds.AddRange(Species.GetActorIds());
    }
    foreach (VarietyMoveEntity move in Moves)
    {
      if (move.Move is not null)
      {
        actorIds.AddRange(move.Move.GetActorIds());
      }
    }
    return actorIds;
  }

  public void SetKey(VarietyKeyChanged @event)
  {
    base.Update(@event);

    Key = @event.Key.Value;
  }

  public void Update(VarietyUpdated @event)
  {
    base.Update(@event);

    if (@event.Name is not null)
    {
      Name = @event.Name.Value?.Value;
    }
    if (@event.Genus is not null)
    {
      Genus = @event.Genus.Value?.Value;
    }
    if (@event.Description is not null)
    {
      Description = @event.Description.Value?.Value;
    }

    if (@event.GenderRatio is not null)
    {
      GenderRatio = @event.GenderRatio.Value?.Value;
    }

    if (@event.CanChangeForm.HasValue)
    {
      CanChangeForm = @event.CanChangeForm.Value;
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
