using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesUpdated(
  Name? Name,
  Summary? Summary,
  Content? Content,
  Friendship BaseFriendship,
  CatchRate CatchRate,
  GrowthRate GrowthRate,
  SpeciesEggs Eggs) : DomainEvent;
