using PokeGame.Core.Rosters;

namespace PokeGame.Core.Trainers;

public sealed class PokemonPartyFullException : ConflictException
{
  public PokemonPartyFullException(Trainer trainer, Roster roster)
    : base("The trainer’s Pokémon party is full.")
  {
    Data["WorldId"] = trainer.WorldId.EntityId;
    Data["TrainerId"] = trainer.EntityId;
    Data["PartyLimit"] = trainer.PartyLimit ?? Roster.PartyLimit;
    Data["PartyCount"] = roster.PartyIds.Count;
  }
}
