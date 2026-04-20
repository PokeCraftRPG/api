using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public class Roster : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Roster";

  public new RosterId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<PokemonSlot, PokemonId> _pokemon = [];
  private readonly Dictionary<PokemonId, PokemonSlot> _slots = [];

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

  public void Add(Specimen specimen, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, specimen.Id, nameof(specimen));

    if (specimen.Ownership is null || specimen.Ownership.TrainerId != TrainerId)
    {
      throw new ArgumentException($"The Pokémon current trainer 'Id={specimen.Ownership?.TrainerId.Value ?? "<null>"}' must be '{TrainerId}'.", nameof(specimen));
    }
    else if (!_slots.ContainsKey(specimen.Id))
    {
      PokemonSlot slot = FindFirstAvailable();
      specimen.Move(slot, userId);
      Raise(new RosterPokemonMoved(specimen.Id, slot), userId.ActorId);
    }
  }

  public void Deposit(Specimen specimen, PokemonParty party, UserId userId)
  {
    if (!_slots.TryGetValue(specimen.Id, out PokemonSlot? previousSlot))
    {
      throw new ArgumentException($"The Pokémon '{specimen}' is not in the trainer 'Id={TrainerId}' roster.", nameof(specimen));
    }
    else if (previousSlot.Box.HasValue)
    {
      throw new PokemonIsNotInPartyException(specimen);
    }

    party.EnsureIsValidWithout(specimen);

    PokemonSlot slot = FindFirstBoxedAvailable() ?? throw new RosterIsFullException(this);
    specimen.Deposit(slot, userId);
    Raise(new RosterPokemonMoved(specimen.Id, slot), userId.ActorId);

    ShiftAfter(party, specimen, previousSlot, userId);
  }

  public void Release(Specimen specimen, PokemonParty party, UserId userId)
  {
    if (!_slots.TryGetValue(specimen.Id, out PokemonSlot? slot))
    {
      throw new ArgumentException($"The Pokémon '{specimen}' is not in the trainer 'Id={TrainerId}' roster.", nameof(specimen));
    }
    else if (!slot.Box.HasValue)
    {
      party.EnsureIsValidWithout(specimen);
    }

    specimen.Release(userId);
    Raise(new RosterPokemonRemoved(specimen.Id), userId.ActorId);

    if (!slot.Box.HasValue)
    {
      ShiftAfter(party, specimen, slot, userId);
    }
  }

  public bool Remove(Specimen specimen, UserId userId) // TODO(fpion): remove this method
  {
    if (!_slots.ContainsKey(specimen.Id))
    {
      return false;
    }

    Raise(new RosterPokemonRemoved(specimen.Id), userId.ActorId);
    return true;
  }
  public void Remove(Specimen specimen, PokemonParty party, UserId userId)
  {
    if (!_slots.TryGetValue(specimen.Id, out PokemonSlot? slot))
    {
      throw new ArgumentException($"The Pokémon '{specimen}' is not in the trainer 'Id={TrainerId}' roster.", nameof(specimen));
    }
    else if (!slot.Box.HasValue)
    {
      party.EnsureIsValidWithout(specimen);
    }

    Raise(new RosterPokemonRemoved(specimen.Id), userId.ActorId);

    if (!slot.Box.HasValue)
    {
      ShiftAfter(party, specimen, slot, userId);
    }
  }
  protected virtual void Handle(RosterPokemonRemoved @event)
  {
    if (_slots.TryGetValue(@event.PokemonId, out PokemonSlot? slot))
    {
      _pokemon.Remove(slot);
    }
    _slots.Remove(@event.PokemonId);
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public IReadOnlyCollection<PokemonId> GetParty() => _slots
    .Where(x => !x.Value.Box.HasValue)
    .OrderBy(x => x.Value.Position)
    .Select(x => x.Key).ToList().AsReadOnly();

  public void Withdraw(Specimen specimen, UserId userId)
  {
    if (!_slots.TryGetValue(specimen.Id, out PokemonSlot? previousSlot))
    {
      throw new ArgumentException($"The Pokémon '{specimen}' is not in the trainer 'Id={TrainerId}' roster.", nameof(specimen));
    }
    else if (!previousSlot.Box.HasValue)
    {
      throw new PokemonAlreadyInPartyException(specimen);
    }

    PokemonSlot slot = FindFirstPartyAvailable() ?? throw new PartyIsFullException(this);
    specimen.Withdraw(slot, userId);
    Raise(new RosterPokemonMoved(specimen.Id, slot), userId.ActorId);
  }

  protected virtual void Handle(RosterPokemonMoved @event)
  {
    if (_slots.TryGetValue(@event.PokemonId, out PokemonSlot? previousSlot))
    {
      _pokemon.Remove(previousSlot);
    }
    _pokemon[@event.Slot] = @event.PokemonId;
    _slots[@event.PokemonId] = @event.Slot;
  }

  private PokemonSlot FindFirstAvailable()
  {
    return FindFirstPartyAvailable() ?? FindFirstBoxedAvailable() ?? throw new RosterIsFullException(this);
  }
  private PokemonSlot? FindFirstBoxedAvailable()
  {
    for (int box = 0; box < PokemonSlot.BoxCount; box++)
    {
      for (int position = 0; position < PokemonSlot.BoxSize; position++)
      {
        PokemonSlot slot = new(position, box);
        if (!_pokemon.ContainsKey(slot))
        {
          return slot;
        }
      }
    }
    return null;
  }
  private PokemonSlot? FindFirstPartyAvailable()
  {
    int position = GetParty().Count;
    return position < PokemonSlot.PartySize ? new PokemonSlot(position) : null;
  }

  private void ShiftAfter(PokemonParty party, Specimen specimen, PokemonSlot previousSlot, UserId userId)
  {
    foreach (Specimen member in party.Members)
    {
      if (!member.Equals(specimen))
      {
        PokemonSlot slot = _slots[member.Id];
        if (slot.IsGreaterThan(previousSlot))
        {
          slot = slot.Previous();

          member.Move(slot, userId);
          Raise(new RosterPokemonMoved(member.Id, slot), userId.ActorId);
        }
      }
    }
  }
}
