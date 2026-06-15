using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesGameDataChanged(Friendship BaseFriendship, CatchRate CatchRate, GrowthRate GrowthRate, EggCycles EggCycles, EggGroups EggGroups) : DomainEvent;
