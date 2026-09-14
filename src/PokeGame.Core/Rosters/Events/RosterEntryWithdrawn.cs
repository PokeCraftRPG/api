using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public sealed record RosterEntryWithdrawn(PokemonId PokemonId) : DomainEvent;
