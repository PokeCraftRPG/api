using Logitar.EventSourcing;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonAcquired(TrainerId TrainerId, PokemonId PokemonId, VarietyId VarietyId) : IEvent
{
  public static PokemonAcquired From(Specimen specimen)
  {
    PokemonOwnership ownership = specimen.Ownership ?? throw new ArgumentException("The ownership is required.", nameof(specimen));
    return new PokemonAcquired(ownership.TrainerId, specimen.Id, specimen.VarietyId);
  }
}
