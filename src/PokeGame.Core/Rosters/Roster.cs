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

  public Roster(Trainer trainer) : base(new RosterId(trainer.Id).StreamId)
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
      Raise(new RosterPokemonAdded(specimen.Id, slot), userId.ActorId);
    }
  }
  protected virtual void Handle(RosterPokemonAdded @event)
  {
    if (_slots.TryGetValue(@event.PokemonId, out PokemonSlot? previousSlot))
    {
      _pokemon.Remove(previousSlot);
    }
    _pokemon[@event.Slot] = @event.PokemonId;
    _slots[@event.PokemonId] = @event.Slot;
  }

  public bool Remove(Specimen specimen, UserId userId)
  {
    if (!_slots.ContainsKey(specimen.Id))
    {
      return false;
    }

    Raise(new RosterPokemonRemoved(specimen.Id), userId.ActorId);
    return true;
  }
  protected virtual void Handle(RosterPokemonRemoved @event)
  {
    if (_slots.TryGetValue(@event.PokemonId, out PokemonSlot? slot))
    {
      _pokemon.Remove(slot);
    }
    _slots.Remove(@event.PokemonId);
  }

  private PokemonSlot FindFirstAvailable()
  {
    return FindFirstPartyAvailable() ?? FindFirstBoxedAvailable() ?? throw new RosterFullException(this);
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

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public IReadOnlyCollection<PokemonId> GetParty() => _slots
    .Where(x => !x.Value.Box.HasValue)
    .OrderBy(x => x.Value.Position)
    .Select(x => x.Key).ToList().AsReadOnly();
}
