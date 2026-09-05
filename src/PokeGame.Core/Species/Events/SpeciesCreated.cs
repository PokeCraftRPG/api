using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesCreated(
  Number Number,
  SpeciesCategory Category,
  Key Key,
  Friendship BaseFriendship,
  CatchRate CatchRate,
  GrowthRate GrowthRate,
  SpeciesEggs Eggs) : DomainEvent;
