using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core;
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
  public string? Summary { get; private set; }
  public string? Content { get; private set; }

  public bool CanChangeForm { get; private set; }
  public int? GenderRatio { get; private set; }
  public string? Genus { get; private set; }

  public List<VarietyMoveEntity> Moves { get; private set; } = [];

  public VarietyEntity(int worldId, int speciesId, VarietyCreated @event) : base(@event)
  {
    WorldId = worldId;
    Id = Entity.Parse(@event.StreamId.Value).Id;

    SpeciesId = speciesId;

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
      actorIds.AddRange(move.GetActorIds());
    }
    return actorIds;
  }

  public void SetDefault(VarietyDefaultChanged @event)
  {
    Update(@event);

    IsDefault = @event.IsDefault;
  }

  public void SetKey(VarietyKeyChanged @event)
  {
    Update(@event);

    Key = @event.Key.Value;
  }

  public void SetMove(int? moveId, VarietyMoveChanged @event)
  {
    Update(@event);

    VarietyMoveEntity? move = Moves.SingleOrDefault(x => x.Id == @event.VarietyMoveId);
    if (move is null)
    {
      if (!moveId.HasValue)
      {
        throw new ArgumentNullException(nameof(moveId));
      }
      move = new VarietyMoveEntity(this, moveId.Value, @event);
      Moves.Add(move);
    }
    else
    {
      move.Update(@event);
    }
  }

  public void Update(VarietyUpdated @event)
  {
    base.Update(@event);

    Name = @event.Name?.Value;
    Summary = @event.Summary?.Value;
    Content = @event.Content?.Value;

    CanChangeForm = @event.CanChangeForm;
    GenderRatio = @event.GenderRatio?.FemaleRate;
    Genus = @event.Genus?.Value;
  }

  public override string ToString() => $"{Name ?? Key} | {base.ToString()}";
}
