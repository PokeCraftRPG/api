using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public sealed class Roster : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Roster";

  public new RosterId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<PokemonId, RosterEntry> _entries = [];
  public IReadOnlyDictionary<PokemonId, RosterEntry> Entries => _entries.AsReadOnly();

  public Roster() : base()
  {
  }

  public Roster(Trainer trainer) : this(trainer.Id)
  {
  }

  public Roster(TrainerId trainerId) : this(new RosterId(trainerId))
  {
  }

  public Roster(RosterId rosterId) : base(rosterId.StreamId)
  {
  }

  public void Add(Specimen specimen, ActorId? actorId = null)
  {
    if (_entries.ContainsKey(specimen.Id))
    {
      throw new ArgumentException($"The Pokémon 'Id={specimen.Id}' is already in the trainer 'Id={TrainerId}' roster.", nameof(specimen));
    }

    bool isInParty = new Random().Next(2) == 1; // TODO(fpion): implement

    Raise(new RosterEntryAdded(specimen.Id, isInParty), actorId);
  }
  private void Handle(RosterEntryAdded @event)
  {
    _entries[@event.PokemonId] = new RosterEntry(@event.IsInParty);
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public void Remove(Specimen specimen, ActorId? actorId = null)
  {
    if (_entries.ContainsKey(specimen.Id))
    {
      Raise(new RosterEntryRemoved(specimen.Id), actorId);
    }
  }
  private void Handle(RosterEntryRemoved @event)
  {
    _entries.Remove(@event.PokemonId);
  }

  public void Swap(Specimen source, Specimen target, ActorId? actorId = null)
  {
    // TODO(fpion): implement
  }
}
