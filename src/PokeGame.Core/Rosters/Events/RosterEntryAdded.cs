using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterEntryAdded(PokemonId PokemonId, bool IsInParty) : DomainEvent;
