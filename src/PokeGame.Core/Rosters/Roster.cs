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

  private readonly Dictionary<Guid, Tag> _tags = [];
  public IReadOnlyDictionary<Guid, Tag> Tags => _tags.AsReadOnly();

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

    if (trainer.Id != TrainerId)
    {
      throw new ArgumentException($"The trainer '{trainer}' was not expected (Id={TrainerId}).", nameof(trainer));
    }

    if (specimen.Ownership?.TrainerId != TrainerId)
    {
      throw new InvalidPokemonOwnerException(this, specimen);
    }
    if (_entries.ContainsKey(specimen.Id))
    {
      throw new PokemonAlreadyInRosterException(this, specimen);
    }

    int partyLimit = trainer.PartyLimit ?? PartyLimit;
    bool isInParty = _partyIds.Count < partyLimit && !specimen.IsEgg;

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

  public void Deposit(Specimen specimen, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, specimen, nameof(specimen));

    if (specimen.Ownership?.TrainerId != TrainerId)
    {
      throw new InvalidPokemonOwnerException(this, specimen);
    }
    if (!_entries.ContainsKey(specimen.Id))
    {
      throw new PokemonNotInRosterException(this, specimen);
    }
    if (!_partyIds.Contains(specimen.Id))
    {
      throw new PokemonNotInPartyException(this, specimen);
    }

    Raise(new RosterEntryDeposited(specimen.Id), actorId);
  }
  private void Handle(RosterEntryDeposited @event)
  {
    _entries[@event.PokemonId] = _entries[@event.PokemonId] with { IsInParty = false };
    _partyIds.Remove(@event.PokemonId);
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public void Remove(Specimen specimen, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, specimen, nameof(specimen));

    if (specimen.Ownership?.TrainerId == TrainerId)
    {
      throw new UnexpectedPokemonOwnerException(this, specimen);
    }
    if (!_entries.ContainsKey(specimen.Id))
    {
      throw new PokemonNotInRosterException(this, specimen);
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
      throw new UnexpectedPokemonOwnerException(this, source);
    }
    if (target.Ownership?.TrainerId != TrainerId)
    {
      throw new InvalidPokemonOwnerException(this, target);
    }

    RosterEntry sourceEntry = _entries.GetValueOrDefault(source.Id) ?? throw new PokemonNotInRosterException(this, source);
    if (_entries.ContainsKey(target.Id))
    {
      throw new UnexpectedPokemonRosterException(this, target);
    }

    Raise(new RosterEntryReplaced(source.Id, target.Id, sourceEntry.IsInParty && !target.IsEgg), actorId);
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
      throw new InvalidPokemonOwnerException(this, source);
    }
    if (target.Ownership?.TrainerId != TrainerId)
    {
      throw new InvalidPokemonOwnerException(this, target);
    }

    RosterEntry sourceEntry = _entries.GetValueOrDefault(source.Id) ?? throw new PokemonNotInRosterException(this, source);
    RosterEntry targetEntry = _entries.GetValueOrDefault(target.Id) ?? throw new PokemonNotInRosterException(this, target);

    if (sourceEntry.IsInParty == targetEntry.IsInParty)
    {
      throw new InvalidRosterSwapException(this, source, target);
    }

    Specimen withdrawn = sourceEntry.IsInParty ? target : source;
    if (withdrawn.IsEgg)
    {
      throw new PokemonEggCannotBeWithdrawnException(this, withdrawn);
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

  public void Withdraw(Specimen specimen, Trainer trainer, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, specimen, nameof(specimen));
    WorldMismatchException.ThrowIfMismatch(this, trainer, nameof(trainer));

    if (trainer.Id != TrainerId)
    {
      throw new ArgumentException($"The trainer '{trainer}' was not expected (Id={TrainerId}).", nameof(trainer));
    }

    if (specimen.Ownership?.TrainerId != TrainerId)
    {
      throw new InvalidPokemonOwnerException(this, specimen);
    }
    if (specimen.IsEgg)
    {
      throw new PokemonEggCannotBeWithdrawnException(this, specimen);
    }
    if (!_entries.ContainsKey(specimen.Id))
    {
      throw new PokemonNotInRosterException(this, specimen);
    }
    if (_partyIds.Contains(specimen.Id))
    {
      throw new PokemonAlreadyInPartyException(this, specimen);
    }

    int partyLimit = trainer.PartyLimit ?? PartyLimit;
    if (_partyIds.Count >= partyLimit)
    {
      throw new PokemonPartyFullException(this, trainer);
    }

    Raise(new RosterEntryWithdrawn(specimen.Id), actorId);
  }
  private void Handle(RosterEntryWithdrawn @event)
  {
    _entries[@event.PokemonId] = _entries[@event.PokemonId] with { IsInParty = true };
    _partyIds.Add(@event.PokemonId);
  }

  #region Tags
  public void AddTag(Tag tag, ActorId? actorId = null) => SetTag(Guid.NewGuid(), tag, actorId);

  public Tag FindTag(Guid id) => TryGetTag(id) ?? throw new InvalidOperationException($"The tag 'Id={id}' was not found on roster 'Id={Id}'.");

  public bool HasTag(Guid id) => _tags.ContainsKey(id);

  public void RemoveTag(Guid id, ActorId? actorId = null)
  {
    if (HasTag(id))
    {
      Raise(new RosterTagRemoved(id), actorId);
    }
  }
  private void Handle(RosterTagRemoved @event)
  {
    _tags.Remove(@event.TagId);
  }

  public void SetTag(Guid id, Tag tag, ActorId? actorId = null)
  {
    Tag? existingTag = TryGetTag(id);
    if (existingTag is null || !Equals(existingTag, tag))
    {
      Raise(new RosterTagChanged(id, tag), actorId);
    }
  }
  private void Handle(RosterTagChanged @event)
  {
    _tags[@event.TagId] = @event.Tag;
  }

  public Tag? TryGetTag(Guid id) => _tags.GetValueOrDefault(id);
  #endregion
}
