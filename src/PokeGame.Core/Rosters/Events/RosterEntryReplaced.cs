using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterEntryReplaced(PokemonId SourceId, PokemonId TargetId, bool IsInParty) : DomainEvent;
