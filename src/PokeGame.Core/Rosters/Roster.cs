using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public sealed class Roster : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Roster";
  public const int PartyLimit = 6;

  public new RosterId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<PokemonId, RosterEntry> _entries = [];
  public IReadOnlyDictionary<PokemonId, RosterEntry> Entries => _entries.AsReadOnly();

  private readonly HashSet<PokemonId> _partyIds = [];
  public IReadOnlySet<PokemonId> PartyIds => _partyIds.AsReadOnly();

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

  public void Add(Specimen specimen, Trainer trainer, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, specimen, nameof(specimen));
    WorldMismatchException.ThrowIfMismatch(this, trainer, nameof(trainer));

    if (specimen.Ownership?.TrainerId != TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={specimen.Id}' should be owned by trainer 'Id={TrainerId}'.", nameof(specimen));
    }
    if (_entries.ContainsKey(specimen.Id))
    {
      throw new ArgumentException($"The Pokémon 'Id={specimen.Id}' is already in trainer’s 'Id={TrainerId}' roster.", nameof(specimen));
    }

    if (trainer.Id != TrainerId)
    {
      throw new ArgumentException($"The trainer '{trainer}' was not expected (Id={TrainerId}).", nameof(trainer));
    }

    int partyLimit = trainer.PartyLimit ?? PartyLimit;
    bool isInParty = _partyIds.Count < partyLimit;

    Raise(new RosterEntryAdded(specimen.Id, isInParty), actorId);
  }
  private void Handle(RosterEntryAdded @event)
  {
    _entries[@event.PokemonId] = new RosterEntry(@event.IsInParty);
    if (@event.IsInParty)
    {
      _partyIds.Add(@event.PokemonId);
    }
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public void Remove(Specimen specimen, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, specimen, nameof(specimen));

    if (specimen.Ownership?.TrainerId == TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={specimen.Id}' should not be owned by trainer 'Id={TrainerId}'.", nameof(specimen));
    }
    if (!_entries.ContainsKey(specimen.Id))
    {
      throw new ArgumentException($"The Pokémon 'Id={specimen.Id}' is not in trainer’s 'Id={TrainerId}' roster.", nameof(specimen));
    }

    Raise(new RosterEntryRemoved(specimen.Id), actorId);
  }
  private void Handle(RosterEntryRemoved @event)
  {
    _entries.Remove(@event.PokemonId);
    _partyIds.Remove(@event.PokemonId);
  }

  public void Replace(Specimen source, Specimen target, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, source, nameof(source));
    WorldMismatchException.ThrowIfMismatch(this, target, nameof(target));

    if (source.Equals(target))
    {
      throw new InvalidOperationException("Exactly two different Pokémon must be specified.");
    }

    if (source.Ownership?.TrainerId == TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={source.Id}' should not be owned by trainer 'Id={TrainerId}'.", nameof(source));
    }
    if (target.Ownership?.TrainerId != TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={target.Id}' should be owned by trainer 'Id={TrainerId}'.", nameof(target));
    }

    RosterEntry sourceEntry = _entries.GetValueOrDefault(source.Id)
      ?? throw new ArgumentException($"The Pokémon 'Id={source.Id}' is not in trainer’s 'Id={TrainerId}' roster.", nameof(source));
    if (_entries.ContainsKey(target.Id))
    {
      throw new ArgumentException($"The Pokémon 'Id={target.Id}' should not be in trainer’s 'Id={TrainerId}' roster.", nameof(target));
    }

    Raise(new RosterEntryReplaced(source.Id, target.Id, sourceEntry.IsInParty), actorId);
  }
  private void Handle(RosterEntryReplaced @event)
  {
    _entries.Remove(@event.SourceId);
    _partyIds.Remove(@event.SourceId);

    _entries[@event.TargetId] = new RosterEntry(@event.IsInParty);
    if (@event.IsInParty)
    {
      _partyIds.Add(@event.TargetId);
    }
  }

  public void Swap(Specimen source, Specimen target, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, source, nameof(source));
    WorldMismatchException.ThrowIfMismatch(this, target, nameof(target));

    if (source.Equals(target))
    {
      throw new InvalidOperationException("Exactly two different Pokémon must be specified.");
    }

    if (source.Ownership?.TrainerId != TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={source.Id}' should be owned by trainer 'Id={TrainerId}'.", nameof(source));
    }
    if (target.Ownership?.TrainerId != TrainerId)
    {
      throw new ArgumentException($"The Pokémon 'Id={target.Id}' should be owned by trainer 'Id={TrainerId}'.", nameof(target));
    }

    RosterEntry sourceEntry = _entries.GetValueOrDefault(source.Id)
      ?? throw new ArgumentException($"The Pokémon 'Id={source.Id}' is not in trainer’s 'Id={TrainerId}' roster.", nameof(source));
    RosterEntry targetEntry = _entries.GetValueOrDefault(target.Id)
      ?? throw new ArgumentException($"The Pokémon 'Id={target.Id}' is not in trainer’s 'Id={TrainerId}' roster.", nameof(target));

    if (sourceEntry.IsInParty == targetEntry.IsInParty)
    {
      throw new NotImplementedException(); // TODO(fpion): 409 Conflict
    }

    Raise(new RosterEntriesSwapped(source.Id, targetEntry.IsInParty, target.Id, sourceEntry.IsInParty), actorId);
  }
  private void Handle(RosterEntriesSwapped @event)
  {
    _entries[@event.SourceId] = _entries[@event.SourceId] with { IsInParty = @event.IsSourceInParty };
    if (@event.IsSourceInParty)
    {
      _partyIds.Add(@event.SourceId);
    }
    else
    {
      _partyIds.Remove(@event.SourceId);
    }

    _entries[@event.TargetId] = _entries[@event.TargetId] with { IsInParty = @event.IsTargetInParty };
    if (@event.IsTargetInParty)
    {
      _partyIds.Add(@event.TargetId);
    }
    else
    {
      _partyIds.Remove(@event.TargetId);
    }
  }
}
