using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesProgressionChanged(Friendship BaseFriendship, CatchRate CatchRate, GrowthRate GrowthRate) : DomainEvent;
