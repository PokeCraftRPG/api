using Logitar;
using Logitar.EventSourcing;
using PokeGame.Core.Moves;

namespace PokeGame.Infrastructure.Entities;

internal class PokemonMoveEntity
{
  public PokemonEntity? Pokemon { get; private set; }
  public int PokemonId { get; private set; }

  public MoveEntity? Move { get; private set; }
  public int MoveId { get; private set; }

  public int LearnedAtLevel { get; private set; }
  public LearningMethod LearningMethod { get; private set; }

  public bool IsMastered { get; private set; }
  public int PowerPointUpgrades { get; private set; }

  public int? Slot { get; private set; }

  public string? CreatedBy { get; private set; }
  public DateTime CreatedOn { get; private set; }

  public string? UpdatedBy { get; private set; }
  public DateTime UpdatedOn { get; private set; }

  public PokemonMoveEntity(PokemonEntity pokemon, int moveId, LearningMethod learningMethod, int? slot, DomainEvent @event)
  {
    Pokemon = pokemon;
    PokemonId = pokemon.PokemonId;

    MoveId = moveId;

    LearnedAtLevel = pokemon.Level;
    LearningMethod = learningMethod;

    Slot = slot;

    CreatedBy = @event.ActorId?.Value;
    CreatedOn = @event.OccurredOn.AsUniversalTime();

    Update(@event);
  }

  private PokemonMoveEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds()
  {
    HashSet<ActorId> actorIds = [];
    if (Move is not null)
    {
      actorIds.AddRange(Move.GetActorIds());
    }
    if (CreatedBy is not null)
    {
      actorIds.Add(new ActorId(CreatedBy));
    }
    if (UpdatedBy is not null)
    {
      actorIds.Add(new ActorId(UpdatedBy));
    }
    return actorIds;
  }

  private void Update(DomainEvent @event)
  {
    UpdatedBy = @event.ActorId?.Value;
    UpdatedOn = @event.OccurredOn.AsUniversalTime();
  }

  public override bool Equals(object? obj) => obj is PokemonMoveEntity entity && entity.PokemonId == PokemonId && entity.MoveId == MoveId;
  public override int GetHashCode() => HashCode.Combine(PokemonId, MoveId);
  public override string ToString() => $"{base.ToString()} (PokemonId={PokemonId}, MoveId={MoveId})";
}
