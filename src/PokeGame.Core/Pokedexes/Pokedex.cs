using Logitar.EventSourcing;
using PokeGame.Core.Pokedexes.Events;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokedexes;

public sealed class Pokedex : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Pokedex";

  public new PokedexId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<VarietyId, PokedexEntryStatus> _entries = [];
  public IReadOnlyDictionary<VarietyId, PokedexEntryStatus> Entries => _entries.AsReadOnly();

  public Pokedex() : base()
  {
  }

  public Pokedex(Trainer trainer) : this(trainer.Id)
  {
  }

  public Pokedex(TrainerId trainerId) : this(new PokedexId(trainerId))
  {
  }

  public Pokedex(PokedexId pokedexId) : base(pokedexId.StreamId)
  {
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public void RegisterAcquired(VarietyId varietyId, ActorId? actorId = null)
  {
    PokedexEntryStatus status = _entries.GetValueOrDefault(varietyId);
    if (status != PokedexEntryStatus.Acquired)
    {
      Raise(new PokedexEntryAcquired(varietyId), actorId);
    }
  }
  private void Handle(PokedexEntryAcquired @event)
  {
    _entries[@event.VarietyId] = PokedexEntryStatus.Acquired;
  }

  public void RegisterSeen(VarietyId varietyId, ActorId? actorId = null)
  {
    if (!_entries.ContainsKey(varietyId))
    {
      Raise(new PokedexEntrySeen(varietyId), actorId);
    }
  }
  private void Handle(PokedexEntrySeen @event)
  {
    _entries[@event.VarietyId] = PokedexEntryStatus.Seen;
  }
}
