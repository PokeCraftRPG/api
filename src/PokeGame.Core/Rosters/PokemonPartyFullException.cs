using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public sealed class PokemonPartyFullException : ConflictException
{
  public PokemonPartyFullException(Roster roster, Trainer trainer)
    : base("The trainer Pokémon party is full.")
  {
    WorldMismatchException.ThrowIfMismatch(roster, trainer, nameof(trainer));

    Data["WorldId"] = trainer.WorldId.EntityId;
    Data["TrainerId"] = trainer.EntityId;
    Data["PartyLimit"] = trainer.PartyLimit ?? Roster.PartyLimit;
    Data["PartyCount"] = roster.PartyIds.Count;
  }
}
