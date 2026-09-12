using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterEntriesSwapped(PokemonId SourceId, bool IsSourceInParty, PokemonId TargetId, bool IsTargetInParty) : DomainEvent;
